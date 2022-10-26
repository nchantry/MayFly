namespace MayFly.Retries.Internal;

internal class RetryBuilder : IRetryBuilder
{
    public RetryBuildContext Context { get; } = new();

    public IRetryBuilder UseDetector(ITransientErrorDetector detector)
    {
        if (detector == null) throw new ArgumentNullException(nameof(detector));
        Context.Detectors.Add(detector);
        return this;
    }

    public IRetryBuilder UseOrchestrator(IRetryOrchestrator orchestrator)
    {
        if (orchestrator == null) throw new ArgumentNullException(nameof(orchestrator));
        Context.Orchestrator = orchestrator;
        return this;
    }

    public IRetryManager Build() => new RetryManager(Context);
}