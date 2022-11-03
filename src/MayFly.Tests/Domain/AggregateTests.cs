using MayFly.Registration;
using MayFly.Tests.Domain.Stubs;
using Microsoft.Extensions.DependencyInjection;

namespace MayFly.Tests.Domain;

public class AggregateTests
{
    public void aggregates_load_from_sources()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddMayFly(cfg =>
        {
            cfg.AddBoundedContext(new BoundedContextName("MyDomain", "MySubDomain"), context =>
            {
                context.AddAggregate<TestAggregate>(aggCfg => aggCfg
                    .AddRoot<TestController>()
                    .AddProjection<TestProjection1>()
                    .AddProjection<TestProjection2>()
                );
            });
        });
    }
}