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
        if (detectorType == null) 
            throw new ArgumentNullException(nameof(detectorType));
        if (!detectorType.IsAssignableTo(typeof(ITransientErrorDetector))) 
            throw new ArgumentOutOfRangeException(nameof(detectorType));
        var instance = (ITransientErrorDetector)Activator.CreateInstance(detectorType)!;
        return builder.UseDetector(instance);
    }
    
    public static IRetryBuilder UseOrchestrator<T>(this IRetryBuilder builder) where T : class, IRetryOrchestrator, new()
    {
        return builder.UseOrchestrator(new T());
    }
    public static IRetryBuilder UseOrchestrator(this IRetryBuilder builder, Type orchestratorType)
    {
        if (orchestratorType == null) 
            throw new ArgumentNullException(nameof(orchestratorType));
        if (!orchestratorType.IsAssignableTo(typeof(IRetryOrchestrator)))
            throw new ArgumentOutOfRangeException(nameof(orchestratorType));

        try
        {
            var instance = (IRetryOrchestrator)Activator.CreateInstance(orchestratorType)!;
            return builder.UseOrchestrator(instance);
        }
        catch
        {
            throw new ArgumentOutOfRangeException(nameof(orchestratorType), "Type could not be constructed");
        }
    }
}