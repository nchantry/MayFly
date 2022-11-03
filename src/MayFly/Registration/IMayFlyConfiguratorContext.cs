namespace MayFly.Registration;

public interface IMayFlyConfiguratorContext
{
    IMayFlyConfiguratorContext AddBoundedContext(BoundedContextName boundedContext, Action<IMayFlyBoundedContextConfiguratorContext> configurator);
}