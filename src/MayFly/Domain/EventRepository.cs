using MayFly.Contracts;

namespace MayFly.Domain;

public class EventRepository : IEventRepository
{
    public Task ReadAsync(Guid streamId)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(Guid streamId, IEvent[] events)
    {
        throw new NotImplementedException();
    }
}