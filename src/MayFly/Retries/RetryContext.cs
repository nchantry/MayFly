namespace MayFly.Retries;

public record RetryContext(int Attempts, DateTime? LastAttemptDateTime, Exception? LastException);
