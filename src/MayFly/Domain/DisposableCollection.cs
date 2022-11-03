namespace MayFly.Domain;

public class DisposableCollection : IDisposable
{
    private readonly IDisposable[] _disposables;
    private bool _disposed = false;

    public DisposableCollection(params IDisposable [] disposables)
    {
        _disposables = disposables;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposed = true;
    }
}