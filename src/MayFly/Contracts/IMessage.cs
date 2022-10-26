namespace MayFly.Contracts;

public interface IMessage
{
    public Guid MessageId { get; set; }    
    public Guid CorrelationId { get; set; }    
}