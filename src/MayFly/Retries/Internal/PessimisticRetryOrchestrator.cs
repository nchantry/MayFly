namespace MayFly.Retries.Internal;

internal class PessimisticRetryOrchestrator : IRetryOrchestrator
{
    public bool ShouldWeTryAgain(RetryContext context) => false;

    public Task? GetPrerequisiteForNextTry(RetryContext context) => Task.CompletedTask;
}