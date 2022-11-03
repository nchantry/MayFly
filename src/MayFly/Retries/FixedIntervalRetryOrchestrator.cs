namespace MayFly.Retries;

public class FixedIntervalRetryOrchestrator : IRetryOrchestrator
{
    private readonly TimeSpan[] _intervals;

    public FixedIntervalRetryOrchestrator(params TimeSpan [] intervals)
    {
        if (intervals == null || intervals.Length == 0) throw new ArgumentNullException(nameof(intervals));
        
        _intervals = intervals;
    }

    public bool ShouldWeTryAgain(RetryContext context) => context.Attempts <= _intervals.Length;

    public Task GetPrerequisiteForNextTry(RetryContext context)
    {
        return !ShouldWeTryAgain(context) ? Task.CompletedTask : Task.Delay(_intervals[context.Attempts - 1]);
    }
}