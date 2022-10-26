namespace MayFly.Contracts;

public interface IAggregateMessage : IMessage
{
    public Guid AggregateId { get; set; }
}