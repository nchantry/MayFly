using MayFly.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;

namespace MayFly.Domain;

public abstract class Aggregate : IAggregate
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MessageLinker _messageLinker;
    private readonly IDisposable _disposable;
    private bool _isDisposed;
    
    protected ILogger Logger { get; }

    public Guid AggregateId { get; }

    public Dictionary<Type, IProjection> Projections { get; } = new();

    public List<IEvent> Stream { get; } = new();

    public IReadOnlyCollection<IEvent> PendingEvents { get; } = new List<IEvent>();

    public long LastPersistedVersion { get; set; }

    public long CurrentVersion { get; set; }
    
    protected Aggregate(IServiceProvider serviceProvider, Guid aggregateId)
    {
        Logger = serviceProvider.GetRequiredService<ILogger>().ForContext(GetType());

        _disposable = new DisposableCollection(
            LogContext.PushProperty(nameof(AggregateId), AggregateId)
        );

        _serviceProvider = serviceProvider;
        _messageLinker = serviceProvider.GetRequiredService<MessageLinker>();
    }

    public void RegisterProjection<T>(T projection) where T : class, IProjection, new() => Projections[typeof(T)] = projection;

    public void RegisterProjection<T>() where T : class, IProjection, new() => RegisterProjection(new T());

    protected T? Projection<T>() => Projections.OfType<T>().FirstOrDefault();

    public async Task LoadAsync(IProjectionSource source)
    {
        Logger.Debug($"{nameof(LoadAsync)}" + " is loading stream {AggregateId}", AggregateId);
        
        await foreach (var @event in source.ReadAsync())
        {
            //RegisterProjection(@event);
        }
    }

    public async Task LoadAsync(IEventStreamSource source)
    {
        Logger.Debug($"{nameof(LoadAsync)}" + " is loading stream {AggregateId}", AggregateId);
        
        await foreach (var @event in source.ReadAsync())
        {
            Stream.Add(@event);
            await ApplyEventAsync(@event);
        }

        CurrentVersion = LastPersistedVersion = Stream.Max(e => e.Version);
    }

    public async Task SaveAsync(IEventStreamSink sink)
    {
        var pendingEvents = PendingEvents.ToArray();
        var totalEvents = pendingEvents.Max();
        var streamVersion = PendingEvents.Max(x => x.Version);

        Logger.Debug(
            $"{nameof(SaveAsync)}" + "is saving to aggregate {AggregateId} a total of {TotalEvents} events, ending in version {MaxVersion}",
            AggregateId, totalEvents, streamVersion
        );
        
        await sink.WriteAsync(pendingEvents).ConfigureAwait(false);
        LastPersistedVersion = streamVersion;
    }

    public async Task ApplyEventAsync(IEvent domainEvent)
    {
        // Logger.Debug(
        //     $"{nameof(ApplyAsync)}" + " {EventId} to {ProjectionType} Projection {ProjectionId}", 
        //     domainEvent.MessageId, typeof(TProjection).FullName, State.Id
        // );

        foreach (var projection in Projections)
        {
            
            await _messageLinker.ApplyAsync(_serviceProvider, projection, domainEvent).ConfigureAwait(false);
        }
    }

    public Task HandleCommandAsync(ICommand command)
    {
        Logger.Debug(
            $"{nameof(HandleCommandAsync)}" + " {EventId} to {AggregateType} Aggregate {AggregateId}", 
            command.MessageId, GetType().FullName, AggregateId
        );

        return _messageLinker.ApplyAsync(_serviceProvider, this, command);
    }

    public virtual void Dispose()
    {
        if (_isDisposed) return;
        
        _disposable?.Dispose();
        _isDisposed = true;
    }

    public virtual ValueTask DisposeAsync()
    {
        if (_isDisposed) return ValueTask.CompletedTask;
        
        _disposable?.Dispose();
        _isDisposed = true;
        
        return ValueTask.CompletedTask;
    }
}
