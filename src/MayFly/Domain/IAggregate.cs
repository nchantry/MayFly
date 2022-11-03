using MayFly.Contracts;

namespace MayFly.Domain;

public interface IAggregate: IDisposable, IAsyncDisposable
{
    Guid AggregateId { get; }

    Task ApplyEventAsync(IEvent domainEvent);
    Task HandleCommandAsync(ICommand @command);
}
