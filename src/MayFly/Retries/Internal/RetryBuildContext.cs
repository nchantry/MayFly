namespace MayFly.Retries.Internal;

internal class RetryBuildContext
{
    public IRetryOrchestrator? Orchestrator { get; set; }
    public List<ITransientErrorDetector> Detectors { get; } = new();
}