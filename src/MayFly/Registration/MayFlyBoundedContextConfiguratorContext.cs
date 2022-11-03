using MassTransit;
using MassTransit.Internals;
using MayFly.Contracts;
using MayFly.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace MayFly.Registration;

public class MayFlyBoundedContextConfiguratorContext : IMayFlyBoundedContextConfiguratorContext
{
    private readonly IServiceCollection _serviceCollection;
    private readonly MessageLinker _messageLinker;
    private readonly ProjectionRegister _projectionRegister;
    private readonly BoundedContextName _boundedContextName;
    private readonly Dictionary<Type, Action<IBusRegistrationContext, IBusFactoryConfigurator, IReceiveEndpointConfigurator>> _buildActions = new();
    private readonly Dictionary<Type, MayFlyAggregateConfiguratorContext> _aggregateContexts = new();

    public MayFlyBoundedContextConfiguratorContext(IServiceCollection serviceCollection, ProjectionRegister projectionRegister, MessageLinker messageLinker, BoundedContextName boundedContextName)
    {
        _serviceCollection = serviceCollection;
        _projectionRegister = projectionRegister;
        _messageLinker = messageLinker;
        _boundedContextName = boundedContextName;
    }

    public IMayFlyBoundedContextConfiguratorContext AddAggregate(Type aggregateType, Action<IMayFlyAggregateConfiguratorContext>? configurator)
    {
        if (configurator != null)
        {
            var context = _aggregateContexts.GetOrAdd(aggregateType, _ => new MayFlyAggregateConfiguratorContext(aggregateType, _projectionRegister));
            configurator(context);
        }

        _messageLinker.RegisterHost(aggregateType, typeof(IMessage), true, "Handle", "HandleAsync");
        var supportedMessages = _messageLinker.GetSupportedMessages();

        foreach (var supportedMessage in supportedMessages)
        {
            var consumerType = typeof(AggregateConsumer<,>).MakeGenericType(aggregateType, supportedMessage);
            _serviceCollection.AddScoped(consumerType, consumerType);
        }
        
        _buildActions[aggregateType] = (busRegistrationContext, busFactoryConfigurator, receiveEndpointConfigurator) =>
        {
            foreach (var supportedMessage in supportedMessages)
            {
                var consumerType = typeof(AggregateConsumer<,>).MakeGenericType(aggregateType, supportedMessage);
                receiveEndpointConfigurator.Consumer(consumerType, busRegistrationContext.GetRequiredService);
                var aggContext = _aggregateContexts[aggregateType];
                aggContext.Build(busRegistrationContext, busFactoryConfigurator, receiveEndpointConfigurator);
            }
        };
        
        return this;
    }

    public void Build(IBusRegistrationContext context, IBusFactoryConfigurator configurator)
    {
        var boundContextQueue = $"queue:{_boundedContextName.Domain}-{_boundedContextName.SubDomain}";

        foreach (var buildActions in _buildActions)
        {
            var aggregateQueue = $"{boundContextQueue}-{buildActions.Key.Name}";
            configurator.ReceiveEndpoint(aggregateQueue, receiveEndpointConfigurator =>
            {
                buildActions.Value(context, configurator, receiveEndpointConfigurator);
            });
        }
    }
}