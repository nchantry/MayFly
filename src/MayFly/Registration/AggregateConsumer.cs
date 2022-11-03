using MassTransit;
using MayFly.Contracts;
using MayFly.Domain;

namespace MayFly.Registration;

public class AggregateConsumer<TAggregate, TCommand> : IConsumer<TCommand>
    where TAggregate : Aggregate
    where TCommand : class, IMessage
{
    public Task Consume(ConsumeContext<TCommand> context)
    {
        throw new NotImplementedException();
    }
}