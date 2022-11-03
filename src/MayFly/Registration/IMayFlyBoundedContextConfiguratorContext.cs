namespace MayFly.Registration;

public interface IMayFlyBoundedContextConfiguratorContext
{
    IMayFlyBoundedContextConfiguratorContext AddAggregate(Type aggregateType, Action<IMayFlyAggregateConfiguratorContext>? configurator);
}