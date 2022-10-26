using MayFly.Contracts;

namespace MayFly.Domain;

public interface IEventStreamSink
{
    Task WriteAsync(params IEvent[] events);
}