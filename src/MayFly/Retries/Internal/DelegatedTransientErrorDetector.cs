namespace MayFly.Retries.Internal;

internal class DelegatedTransientErrorDetector : ITransientErrorDetector
{
    private readonly Func<Exception, bool> _detector;

    public DelegatedTransientErrorDetector(Func<Exception, bool> detector)
    {
        _detector = detector;
    }
    
    public bool IsTransient(Exception ex)
    {
        return _detector.Invoke(ex);
    }
}