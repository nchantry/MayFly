using MassTransit;

namespace MayFly.Registration;

public class MayFlyAggregateConfiguratorContext : IMayFlyAggregateConfiguratorContext
{
    private readonly Type _aggregateType;
    private readonly ProjectionRegister _projectionRegister;
    private Action<MayFlyReceiveEndpointConfigurator> _receiveEndpointCallback = _ => { };

    public MayFlyAggregateConfiguratorContext(Type aggregateType, ProjectionRegister projectionRegister)
    {
        _aggregateType = aggregateType;
        _projectionRegister = projectionRegister;
    }
    
    /// <summary>
    /// Registers a Web API controller to act as an aggregate root for the aggregate.  All commands and queries
    /// intended for the aggregate will be routed via this controller into the endpoint managed by the aggregate.
    /// </summary>
    /// <returns></returns>
    public IMayFlyAggregateConfiguratorContext AddRoot(Type controllerType)
    {
        return this;
    }

    /// <summary>
    /// Registers a projection type with the aggregate, through which events created by the aggregate will be streamed
    /// and their results persisted as documents and published.
    /// </summary>
    /// <param name="projectionType"></param>
    /// <returns></returns>
    public IMayFlyAggregateConfiguratorContext AddProjection(Type projectionType)
    {
        return this;
    }

    /// <summary>
    /// Provides the developer the ability to extend the 
    /// </summary>
    /// <param name="configurator"></param>
    /// <returns></returns>
    public IMayFlyAggregateConfiguratorContext ConfigureEndpoint(Action<MayFlyReceiveEndpointConfigurator>? configurator)
    {
        if (configurator != null)
        {
            _receiveEndpointCallback = configurator;
        }
        return this;
    }

    public void Build(
        IBusRegistrationContext busRegistrationContext, 
        IBusFactoryConfigurator busFactoryConfigurator, 
        IReceiveEndpointConfigurator receiveEndpointConfigurator
    )
    {
        if (busRegistrationContext == null) throw new ArgumentNullException(nameof(busRegistrationContext));
        if (busFactoryConfigurator == null) throw new ArgumentNullException(nameof(busFactoryConfigurator));
        if (receiveEndpointConfigurator == null) throw new ArgumentNullException(nameof(receiveEndpointConfigurator));

        var masstransitContext = new MayFlyReceiveEndpointConfigurator(busRegistrationContext, receiveEndpointConfigurator);
        _receiveEndpointCallback(masstransitContext);
    }
}