namespace MayFly.Retries;

public interface IRetryOrchestrator
{
    bool ShouldWeTryAgain(RetryContext context);
    Task GetPrerequisiteForNextTry(RetryContext context);
}