using System.Collections.Concurrent;
using System.Reflection;
using MayFly.Retries.Internal;

namespace MayFly.Retries;

public static class RetryBuilderExtensions
{
    public static IRetryBuilder UseDetector(this IRetryBuilder builder, Func<Exception, bool> detector)
    {
        return builder.UseDetector(new DelegatedTransientErrorDetector(detector));
    }
    public static IRetryBuilder UseDetector<T>(this IRetryBuilder builder) where T : class, ITransientErrorDetector, new()
    {
        return builder.UseDetector(new T());
    }
    public static IRetryBuilder UseDetector(this IRetryBuilder builder, Type detectorType)
    {
        if (detectorType == null) throw new ArgumentNullException(nameof(detectorType));

        return builder.UseDetectors(detectorType);
    }
    
    public static IRetryBuilder UseDetectors(this IRetryBuilder builder, params Type[] detectorTypes)
    {
        // Disabled as resharper is wrong
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        detectorTypes = detectorTypes?.Where(x => x != null).ToArray() ?? Array.Empty<Type>();
        
        // Disabled as resharper is wrong
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (!detectorTypes.Any() || detectorTypes.Any(x => x == null)) 
            throw new ArgumentNullException(nameof(detectorTypes));

        if (detectorTypes.Any(x => x.IsAssignableTo(typeof(ITransientErrorDetector)) != true))
            throw new ArgumentOutOfRangeException(nameof(detectorTypes), $"Detectors must implement {nameof(ITransientErrorDetector)}");
        
        foreach (var detectorType in detectorTypes)
        {
            var instance = (ITransientErrorDetector)Activator.CreateInstance(detectorType)!;
            builder = builder.UseDetector(instance);
        }

        return builder;
    }
    
    private static readonly ConcurrentDictionary<Type, MethodInfo> _methodCache = new();
    private static readonly MethodInfo UseOrchestratorCommon = typeof(RetryBuilderExtensions)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Single(x => x.Name == nameof(UseOrchestrator) && x.IsGenericMethod && x.GetParameters().Length == 1);
    
    public static IRetryBuilder UseOrchestrator<T>(this IRetryBuilder builder) where T : class, IRetryOrchestrator, new()
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        return builder.UseOrchestrator(() => new T());
    }

    public static IRetryBuilder UseOrchestrator(this IRetryBuilder builder, Type orchestratorType)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (orchestratorType == null) 
            throw new ArgumentNullException(nameof(orchestratorType));
        if (!orchestratorType.IsAssignableTo(typeof(IRetryOrchestrator)))
            throw new ArgumentOutOfRangeException(nameof(orchestratorType));

        var method = _methodCache.GetOrAdd(orchestratorType, key => UseOrchestratorCommon.MakeGenericMethod(key));
        
        return (IRetryBuilder)method.Invoke(null, new object[] { builder })!;
    }

    public static IRetryBuilder UseOrchestrator<T>(this IRetryBuilder builder, Func<T> factory) where T : IRetryOrchestrator
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        
        try
        {
            var instance = factory() ?? throw new ArgumentOutOfRangeException(nameof(factory), "The factory returned a null orchestrator");
            return builder.UseOrchestrator(instance);
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException("The factory failed when creating an orchestrator", ex);
        }
    }
}