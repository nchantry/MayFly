namespace MayFly.Registration;

public interface IMayFlyAggregateConfiguratorContext
{
    /// <summary>
    /// Registers a Web API controller to act as an aggregate root for the aggregate.  All commands and queries
    /// intended for the aggregate will be routed via this controller into the endpoint managed by the aggregate.
    /// </summary>
    /// <returns></returns>
    IMayFlyAggregateConfiguratorContext AddRoot(Type controllerType);

    /// <summary>
    /// Registers a projection type with the aggregate, through which events created by the aggregate will be streamed
    /// and their results persisted as documents and published.
    /// </summary>
    /// <param name="projectionType"></param>
    /// <returns></returns>
    IMayFlyAggregateConfiguratorContext AddProjection(Type projectionType);

    /// <summary>
    /// Provides the developer the ability to extend the 
    /// </summary>
    /// <param name="configurator"></param>
    /// <returns></returns>
    IMayFlyAggregateConfiguratorContext ConfigureEndpoint(Action<MayFlyReceiveEndpointConfigurator>? configurator);
}