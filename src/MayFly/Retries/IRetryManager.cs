namespace MayFly.Retries;

public interface IRetryManager
{
    void Run(Action<RetryContext> callback);
    T Run<T>(Func<RetryContext, T> callback);
    Task RunAsync(Func<RetryContext, Task> callback);
    Task<T> RunAsync<T>(Func<RetryContext, Task<T>> callback);
}