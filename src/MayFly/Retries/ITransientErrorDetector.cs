namespace MayFly.Retries;

public interface ITransientErrorDetector
{
    bool IsTransient(Exception ex);
}