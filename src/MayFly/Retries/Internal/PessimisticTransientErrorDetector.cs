namespace MayFly.Retries.Internal;

internal class PessimisticTransientErrorDetector : ITransientErrorDetector
{
    public bool IsTransient(Exception ex) => false;
}