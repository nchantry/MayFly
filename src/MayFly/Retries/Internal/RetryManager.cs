namespace MayFly.Retries.Internal;

internal class RetryManager : IRetryManager
{
    internal List<ITransientErrorDetector> Detectors { get; }
    internal IRetryOrchestrator Orchestrator { get; }

    internal RetryManager(RetryBuildContext retryBuildContext)
    {
        Detectors = retryBuildContext.Detectors;
        Orchestrator = retryBuildContext.Orchestrator ?? new PessimisticRetryOrchestrator();

        if (!Detectors.Any())
        {
            Detectors.Add(new PessimisticTransientErrorDetector());
        }
    }
    
    public void Run(Action<RetryContext> callback)
    {
        Run(ctx =>
        {
            callback(ctx);
            return true;
        });
    }

    public T Run<T>(Func<RetryContext, T> callback)
    {
        Task? waitForIt = default;
        var retryContext = new RetryContext(0, default, default);
        while (true)
        {
            try
            {
                if (waitForIt != default) waitForIt.Wait();
                return callback(retryContext);
            }
            catch (Exception e)
            {
                retryContext = new RetryContext(retryContext.Attempts + 1, DateTime.Now, e);
                if (!Detectors.Any(x => x.IsTransient(e))) throw;
                if (!Orchestrator.ShouldWeTryAgain(retryContext)) throw;
                waitForIt = Orchestrator.GetPrerequisiteForNextTry(retryContext);
            }
        }
    }

    public async Task RunAsync(Func<RetryContext, Task> callback)
    {
        await RunAsync(async ctx =>
        {
            await callback(ctx);
            return true;
        });
    }

    public async Task<T> RunAsync<T>(Func<RetryContext, Task<T>> callback)
    {
        Task? waitForIt = default;
        var retryContext = new RetryContext(0, default, default);
        while (true)
        {
            try
            {
                if (waitForIt != default) await waitForIt;
                return await callback(retryContext);
            }
            catch (Exception e)
            {
                retryContext = new RetryContext(retryContext.Attempts + 1, DateTime.Now, e);
                if (!Detectors.Any(x => x.IsTransient(e))) throw;
                if (!Orchestrator.ShouldWeTryAgain(retryContext)) throw;
                waitForIt = Orchestrator.GetPrerequisiteForNextTry(retryContext);
            }
        }
    }
}