namespace MayFly.Contracts;

public interface IEvent : IMessage
{
    long Version { get; }
}