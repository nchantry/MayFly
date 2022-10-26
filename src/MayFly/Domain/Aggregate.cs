using MayFly.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MayFly.Domain;

public abstract class Aggregate : IAggregate, IDisposable, IAsyncDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MessageLinker _messageLinker;
    private readonly IDisposable _loggingContext;
    private bool _isDisposed;
    
    protected ILogger Logger { get; }

    public Guid AggregateId { get; }

    protected Aggregate(IServiceProvider serviceProvider, Guid aggregateId)
    {
        Logger = serviceProvider.GetRequiredService<ILogger<Aggregate>>();
        _serviceProvider = serviceProvider;
        _messageLinker = serviceProvider.GetRequiredService<MessageLinker>();
        _loggingContext = Logger.BeginScope("{AggregateId}", aggregateId.ToString());
    }
    
    public virtual Task ApplyAsync(IEvent @event)
    {
        return _messageLinker.ApplyAsync(_serviceProvider, this, @event);
    }

    public virtual Task HandleAsync(ICommand command)
    {
        return _messageLinker.ApplyAsync(_serviceProvider, this, command);
    }


    public virtual void Dispose()
    {
        if (!_isDisposed)
        {
            _loggingContext?.Dispose();
            _isDisposed = true;
        }
    }

    public virtual ValueTask DisposeAsync()
    {
        if (!_isDisposed)
        {
            _loggingContext?.Dispose();
            _isDisposed = true;
        }
        return ValueTask.CompletedTask;
    }
}

public abstract class Aggregate<TRoot, TProjection> : Aggregate, IAggregate<TRoot, TProjection> 
    where TRoot : IEntity
    where TProjection : IProjection
{
    public TRoot Root { get; set; }
    public TProjection Projection { get; set; }

    public List<IEvent> Stream { get; } = new();

    public IEvent [] PendingEvents { get; set; }

    public long LastPersistedVersion { get; set; }

    public long CurrentVersion { get; set; }

    
    protected Aggregate(IServiceProvider serviceProvider, Guid rootId) : base(serviceProvider, rootId)
    {
        
    }

    public override Task ApplyAsync(IEvent @event)
    {
        Logger.LogDebug($"{nameof(ApplyAsync)}" + " {EventId} to {ProjectionType} Projection {ProjectionId}", 
            @event.MessageId, typeof(TProjection).FullName, Projection.Id);
        
        return base.ApplyAsync(@event);
    }

    public override Task HandleAsync(ICommand command)
    {
        Logger.LogDebug($"{nameof(HandleAsync)}" + " {EventId} to {AggregateType} Aggregate {AggregateId}", 
            @command.MessageId, GetType().FullName, AggregateId);

        return base.HandleAsync(command);
    }

    public async Task LoadAsync(IEventStreamSource source)
    {
        await foreach (var @event in source.ReadAsync())
        {
            Stream.Add(@event);
            await ApplyAsync(@event);
        }

        CurrentVersion = LastPersistedVersion = Stream.Max(e => e.Version);
    }

    public async Task SaveAsync(IEventStreamSink sink)
    {
        await sink.WriteAsync(PendingEvents.ToArray()).ConfigureAwait(false);
        LastPersistedVersion = PendingEvents.Max(e => e.Version);
    }
}
