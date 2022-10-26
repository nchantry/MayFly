using MayFly.Contracts;

namespace MayFly.Domain;

public interface IAggregate
{
    Guid AggregateId { get; }

    Task ApplyAsync(IEvent @event);
    Task HandleAsync(ICommand @command);
}

public interface IAggregate<TRoot, TProjection> : IAggregate 
    where TRoot : IEntity
    where TProjection : IProjection
{
    TRoot Root { get; set; }
}