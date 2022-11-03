using System.Collections.Concurrent;
using System.Reflection;

namespace MayFly.Helpers;

public static class TypeHelper
{
    private static readonly ConcurrentDictionary<Type, Func<Type, IServiceProvider, object?>> _typeCache = new();

    public static T Create<T>()
    {
        var type = GetType(typeof(T));
        var inst = (T)Activator.CreateInstance(type)!;
        return inst;
    }

    public static T Create<T>(Action<T> initializer)
    {
        var inst = Create<T>();
        initializer?.Invoke(inst);
        return inst;
    }

    public static T? Create<T>(IServiceProvider serviceProvider, Action<T>? initializer = null)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        var implementationType = GetType(typeof(T));

        var factory = GetTypeFactory<T>(implementationType);

        var inst = factory(implementationType, serviceProvider);

        if (inst is T typedInstance)
        {
            initializer?.Invoke(typedInstance);
            return typedInstance;
        }

        return default;
    }

    private static Func<Type, IServiceProvider, object?> GetTypeFactory<T>(Type implementationType)
    {
        var factory = _typeCache.GetOrAdd(implementationType, (t, sp) =>
        {
            // First try to resolve via the IServiceProvider
            var instance = sp.GetService(t);
            if (instance != null) return instance;

            var constructors = t.GetConstructors().OrderBy(x => x.GetParameters().Length).ToArray();
            if (constructors.Length == 0 || constructors.First().GetParameters().Length == 0)
            {
                return Activator.CreateInstance(t)!;
            }

            foreach (var ctor in constructors)
            {
                var parameters = ctor.GetParameters().Select(p => sp.GetService(p.ParameterType)).ToArray();
                if (parameters.All(pv => pv != null))
                {
                    return Activator.CreateInstance(t, parameters)!;
                }
            }
            return null;
        });
        return factory;
    }

    public static Type GetType<T>()
    {
        var type = typeof(T);
        return GetType(type);
    }

    public static Type GetType(Type type)
    {
        return type.IsClass || type.ContainsGenericParameters ? type : MassTransit.Metadata.TypeMetadataCache.GetImplementationType(type);
    }

    public static Type[] GetSubclassTypes<TBaseClass>(params Assembly[] assemblies)
    {
        return assemblies.SelectMany(x => x.GetTypes().Where(x => x.IsSubclassOf(typeof(TBaseClass)))).ToArray();
    }

    public static Type[] GetTypesImplementingInterface<TInterface>(params Assembly[] assemblies)
    {
        return assemblies.SelectMany(x => x.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(TInterface)))).ToArray();
    }
}