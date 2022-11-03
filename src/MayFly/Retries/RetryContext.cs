namespace MayFly.Retries;

public record RetryContext(int Attempts, DateTime FirstAttempt, DateTime? LastAttemptDateTime, Exception? LastException)
{
    public TimeSpan Elapsed() => LastAttemptDateTime?.Subtract(FirstAttempt) ?? TimeSpan.Zero;
}
