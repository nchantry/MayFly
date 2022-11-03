using MassTransit;
using MassTransit.Internals;
using MayFly.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace MayFly.Registration;

public class MayFlyConfiguratorContext : IMayFlyConfiguratorContext
{
    private readonly Dictionary<BoundedContextName, MayFlyBoundedContextConfiguratorContext> _boundedContextConfiguratorContexts = new();
    private readonly IServiceCollection _serviceCollection;
    private readonly ProjectionRegister _projectionRegister;
    private readonly MessageLinker _messageLinker;

    public MayFlyConfiguratorContext(IServiceCollection serviceCollection, ProjectionRegister projectionRegister, MessageLinker messageLinker)
    {
        _serviceCollection = serviceCollection;
        _projectionRegister = projectionRegister;
        _messageLinker = messageLinker;
    }

    public IMayFlyConfiguratorContext AddBoundedContext(BoundedContextName boundedContext, Action<IMayFlyBoundedContextConfiguratorContext> configurator)
    {
        if (boundedContext == null) throw new ArgumentNullException(nameof(boundedContext));
        if (configurator == null) throw new ArgumentNullException(nameof(configurator));

        var context = _boundedContextConfiguratorContexts.GetOrAdd(boundedContext, key => new(_serviceCollection, _projectionRegister, _messageLinker, key));
        configurator(context);

        return this;
    }

    public void Build(IServiceCollection serviceCollection)
    {
        if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

        serviceCollection.AddMassTransit(configurator =>
        {
            configurator.UsingInMemory((busRegistrationContext, busFactoryConfigurator) =>
            {
                foreach (var context in _boundedContextConfiguratorContexts.Values)
                {
                    context.Build(busRegistrationContext, busFactoryConfigurator);
                }
            });
            // configurator.UsingRabbitMq((busRegistrationContext, busFactoryConfigurator) =>
            // {
            //     busFactoryConfigurator.Host(new Uri("rabbitmq:localhost"), cfg =>
            //     {
            //         cfg.Username("guest");
            //         cfg.Password("guest");
            //     });
            // });
        });
    }
}