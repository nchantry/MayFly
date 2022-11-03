using System.Web.Http;
using MayFly.Domain;

namespace MayFly.Registration;

public static class MayFlyAggregateConfiguratorContextExtensions
{
    public static IMayFlyAggregateConfiguratorContext AddRoot<T>(this IMayFlyAggregateConfiguratorContext register) where T : ApiController
    {
        if (register == null) throw new ArgumentNullException(nameof(register));
        return register.AddRoot(typeof(T));
    }
    
    public static IMayFlyAggregateConfiguratorContext AddProjection<T>(this IMayFlyAggregateConfiguratorContext register) where T : IProjection
    {
        if (register == null) throw new ArgumentNullException(nameof(register));
        
        return register.AddProjection(typeof(T));
    }
    
    public static IMayFlyAggregateConfiguratorContext AddProjections(this IMayFlyAggregateConfiguratorContext register, params Type? [] projectionTypes)
    {
        if (register == null) throw new ArgumentNullException(nameof(register));
        if (projectionTypes == null) throw new ArgumentNullException(nameof(projectionTypes));

        projectionTypes = projectionTypes.Where(x => x != null).ToArray();
        
        if (!projectionTypes.Any()) throw new ArgumentNullException(nameof(projectionTypes));

        foreach (var projectionType in projectionTypes)
        {
            register = register.AddProjection(projectionType!);
        }

        return register;
    }
}