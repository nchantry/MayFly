using System.Collections.Concurrent;
using System.Reflection;
using Marten.Metadata;
using MayFly.Contracts;
using MayFly.Helpers;

namespace MayFly.Domain;

public delegate Task PrismMessageHandler(IServiceProvider serviceProvider, object instance, IMessage message);

public class MessageLinker
{
    internal const string UnsupportedMessageTypeError = "The specified message type is not handled by host.";

    private readonly ConcurrentDictionary<Type, PrismMessageHandler> _applicators = new();

    public Type[] RegisterHost(Type hostType, Type messageConstraintType, bool incrementHostVersionOnApply, params string[] methodNames)
    {
        if (hostType == null) throw new ArgumentNullException(nameof(hostType));
        if (messageConstraintType == null) throw new ArgumentNullException(nameof(messageConstraintType));
        if (methodNames == null) throw new ArgumentNullException(nameof(methodNames));

        methodNames = methodNames.Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();

        if (!methodNames.Any()) throw new ArgumentNullException(nameof(methodNames));

        if (!messageConstraintType.IsAssignableTo(typeof(IMessage)))
        {
            throw new ArgumentException(
                $"The specified {messageConstraintType} must be assignable to {nameof(IMessage)}.",
                nameof(messageConstraintType)
            );
        }

        var allMethods = (
            from method in hostType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            where methodNames.Contains(method.Name)
            let parameters = method.GetParameters().Where(p => p.ParameterType != messageConstraintType).ToArray()
            let messageParam = parameters.SingleOrDefault(p => p.ParameterType.IsAssignableTo(messageConstraintType))
            where messageParam != null
            orderby messageParam.ParameterType.FullName, parameters.Length descending
            select new { Method = method, MessageType = messageParam, Parameters = parameters }
        ).ToList();

        foreach (var method in allMethods)
        {
            var messageParam = method.MessageType;
            var methodInfo = method.Method;

            Task Handle(IServiceProvider sp, object host, IMessage msg)
            {
                var parameters = method
                    .Parameters
                    .Select(pi => pi == messageParam ? msg : sp.GetService(pi.ParameterType))
                    .ToArray();

                if (methodInfo.ReturnType.IsAssignableTo(typeof(void)))
                {
                    methodInfo.Invoke(host, parameters.ToArray());
                    if (incrementHostVersionOnApply && host is IVersioned versionedHost && msg is IVersioned versionedMessage)
                    {
                        versionedHost.Version = versionedMessage.Version;
                    }
                    return Task.CompletedTask;
                }
                if (methodInfo.ReturnType.IsAssignableTo(typeof(Task)))
                {
                    var task = (Task)methodInfo.Invoke(host, parameters.ToArray())!;
                    if (incrementHostVersionOnApply && host is IVersioned versionedHost && msg is IVersioned versionedMessage)
                    {
                        versionedHost.Version = versionedMessage.Version;
                    }
                    return task;
                }
                else
                {
                    var result = methodInfo.Invoke(host, parameters.ToArray());
                    if (incrementHostVersionOnApply && host is IVersioned versionedHost && msg is IVersioned versionedMessage)
                    {
                        versionedHost.Version = versionedMessage.Version;
                    }
                    return Task.FromResult(result);
                }
            }

            var compiledType = messageParam.ParameterType;
            var runtimeType = TypeHelper.GetType(compiledType);

            _applicators.AddOrUpdate(compiledType, _ => Handle, (_, e) => e);
            if (compiledType != runtimeType)
            {
                // Ok, so why are we double keying this handler?  It comes down to the way MassTransit manages messages
                // as interfaces.  We will often write message handlers against an interface of the message - not a concrete
                // type.  In fact, at compile time, we often won't even have a class - i.e. it doesn't exist outside of runtime.
                // This is because we don't need a concrete type and MassTransit provides the convenience of auto-creating
                // concrete types for you for pure POCOs such as messages.  So, when a message comes in via a Consumer to apply
                // to some sort of model, be it a Projection or an Aggregate, we look at its type, and its type will always be a
                // class, not an interface.  So, double keying against both compile time and runtime time data contracts makes
                // for easier matching of handlers to messages.
                _applicators.AddOrUpdate(runtimeType, _ => _applicators[compiledType], (_, _) => _applicators[compiledType]);
            }
        }

        return GetSupportedMessages();
    }

    public Task ApplyAsync(IServiceProvider serviceProvider, object host, IMessage message)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        if (host == null) throw new ArgumentNullException(nameof(host));
        if (message == null) throw new ArgumentNullException(nameof(message));

        var messageType = message.GetType();
        if (!_applicators.TryGetValue(messageType, out var applicator))
        {
            // Sometimes contracts don't match exactly - we may have a derived class hitting
            // a base class handler.  This should be rare, and as we double key messages handlers
            // against both their compile time type and their default runtime type implementation,
            // the below should only be rarely needed - but it is a possible scenario hence we
            // should cater for it.
            foreach (var app in _applicators)
            {
                if (messageType.IsAssignableTo(app.Key))
                {
                    applicator = app.Value;
                    break;
                }
            }
        }

        if (applicator == default)
        {
            throw new NotSupportedException(UnsupportedMessageTypeError);
        }

        var result = applicator.Invoke(serviceProvider, host, message);
        return result;
    }

    public async Task ApplyAsync(IServiceProvider serviceProvider, object host, IMessage[] messages)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        if (host == null) throw new ArgumentNullException(nameof(host));
        if (messages == null || !messages.Any()) throw new ArgumentNullException(nameof(messages));

        var exceptions = new List<Exception>();
        foreach (var message in messages)
        {
            try
            {
                await ApplyAsync(serviceProvider, host, message);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }

        }
        if (exceptions.Any())
        {
            if (exceptions.Count == 1) throw exceptions[0];
            throw new AggregateException("One or more messages could not be applied.", exceptions);
        }
    }

    public Type[] GetSupportedMessages() => _applicators.Keys.OrderBy(x => x.FullName).ToArray();
}
