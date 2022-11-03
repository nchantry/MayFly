namespace MayFly.Domain;

public interface IProjectionSource
{
    IAsyncEnumerable<IProjection> ReadAsync();
}