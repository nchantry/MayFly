namespace MayFly.Retries;

public static class RetryBuilderDetectorExtensions
{
    public static IRetryBuilder UseFixedIntervalRetryOrchestrator(this IRetryBuilder builder, params TimeSpan [] intervals)
    {
        return builder.UseOrchestrator(() => new FixedIntervalRetryOrchestrator(intervals));
    }
    
    public static IRetryBuilder UseExponentialBackoffRetryOrchestrator(this IRetryBuilder builder, int maxRetries, TimeSpan minimumDelay, TimeSpan maximumTotalDelay)
    {
        return builder.UseOrchestrator(() => new ExponentialBackoffRetryOrchestrator(maxRetries, minimumDelay, maximumTotalDelay));
    }
}