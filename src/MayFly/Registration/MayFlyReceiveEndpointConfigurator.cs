using MassTransit;

namespace MayFly.Registration;

public class MayFlyReceiveEndpointConfigurator
{
    public IBusRegistrationContext Context { get; }
    public IReceiveEndpointConfigurator Configurator { get; }

    public MayFlyReceiveEndpointConfigurator(IBusRegistrationContext busRegistrationContext, IReceiveEndpointConfigurator receiveEndpointConfigurator)
    {
        Context = busRegistrationContext ?? throw new ArgumentNullException(nameof(busRegistrationContext));
        Configurator = receiveEndpointConfigurator ?? throw new ArgumentNullException(nameof(receiveEndpointConfigurator));
    }
}