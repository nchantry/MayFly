using MayFly.Contracts;

namespace MayFly.Domain;

public interface IProjection : IVersioned
{
    Guid Id { get; }
}