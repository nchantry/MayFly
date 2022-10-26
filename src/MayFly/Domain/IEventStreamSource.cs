using MayFly.Contracts;

namespace MayFly.Domain;

public interface IEventStreamSource
{
    IAsyncEnumerable<IEvent> ReadAsync();
}