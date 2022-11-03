using MayFly.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace MayFly.Registration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMayFly(this IServiceCollection serviceCollection, Action<MayFlyConfiguratorContext> configurator)
    {
        if (configurator == null) throw new ArgumentNullException(nameof(configurator));
        
        var projectionRegister = new ProjectionRegister();
        var messageLinker = new MessageLinker();

        serviceCollection.AddSingleton(messageLinker);
        serviceCollection.AddSingleton(projectionRegister);
        
        var context = new MayFlyConfiguratorContext(serviceCollection, projectionRegister, messageLinker);
        configurator(context);
        context.Build(serviceCollection);

        return serviceCollection;
    }
}