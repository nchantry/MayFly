namespace MayFly.Retries;

public interface IRetryBuilder
{
    IRetryBuilder UseDetector(ITransientErrorDetector detector);
    IRetryBuilder UseOrchestrator(IRetryOrchestrator orchestrator);
    IRetryManager Build();
}