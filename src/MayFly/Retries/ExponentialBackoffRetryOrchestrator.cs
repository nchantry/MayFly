namespace MayFly.Retries;

public class ExponentialBackoffRetryOrchestrator : IRetryOrchestrator
{
    private static readonly Random _random = new();

    private readonly int _maxRetries;
    private readonly TimeSpan _minimumDelay;
    private readonly TimeSpan _maximumTotalDelay;

    public ExponentialBackoffRetryOrchestrator(int maxRetries, TimeSpan minimumDelay, TimeSpan maximumTotalDelay)
    {
        if (maxRetries < 0) throw new ArgumentOutOfRangeException(nameof(maxRetries), "Must be a positive number");
        if (maximumTotalDelay < minimumDelay)
            throw new ArgumentOutOfRangeException(nameof(maximumTotalDelay), $"{nameof(maximumTotalDelay)} cannot be smaller than {nameof(minimumDelay)}");
        
        _maxRetries = maxRetries;
        _minimumDelay = minimumDelay;
        _maximumTotalDelay = maximumTotalDelay;
    }
    
    public bool ShouldWeTryAgain(RetryContext context)
    {
        if (context.Elapsed() >= _maximumTotalDelay) return false;
        return context.Attempts <= _maxRetries;
    }

    public Task GetPrerequisiteForNextTry(RetryContext context)
    {
        if (!ShouldWeTryAgain(context)) return Task.CompletedTask;
        
        var jitter = (long)(_minimumDelay.TotalMilliseconds * _random.NextDouble());
        var interval = (long)Math.Pow(2, context.Attempts - 1) * 1000;
        var delay = TimeSpan.FromMilliseconds(jitter + interval);
        return Task.Delay(delay);
    }
}