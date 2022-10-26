using MayFly.Contracts;

namespace MayFly.Domain;

public interface IProjection : IEventStreamSink
{
    Guid Id { get; }
    Task ApplyAsync(IEvent @event);
}