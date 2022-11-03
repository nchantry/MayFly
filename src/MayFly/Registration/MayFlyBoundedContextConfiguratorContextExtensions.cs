using MayFly.Domain;

namespace MayFly.Registration;

public static class MayFlyBoundedContextConfiguratorContextExtensions
{
    public static IMayFlyBoundedContextConfiguratorContext AddAggregate<T>(
        this IMayFlyBoundedContextConfiguratorContext context, Action<IMayFlyAggregateConfiguratorContext>? configurator) 
        where T : Aggregate
    {
        return context.AddAggregate(typeof(T), configurator);
    }
}