using MayFly.Contracts;

namespace MayFly.Domain;

public interface IEventRepository
{
    Task ReadAsync(Guid streamId);
    Task WriteAsync(Guid streamId, IEvent [] events);
}


public interface IProjectionManifest
{
    IProjection [] CreateProjections();
}

