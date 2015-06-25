using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Subscribers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExecuteSubscribers
    {
        private readonly AppFunc _next;

        public ExecuteSubscribers(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var events = environment.GetEventStoreRequest().Events;
            var onError = environment.GetEventStoreRequest().OnException;

            foreach (var evnt in events)
            {
                Exception lastException = null;
                var retryCount = 0;

                while (retryCount < 5)
                {
                    try
                    {
                        await Execute(evnt.Data, evnt.Metadata, environment);
                        lastException = null;
                        break;
                    }
                    catch (Exception exception)
                    {
                        lastException = exception;
                        retryCount++;
                    }
                }

                if (lastException != null)
                    onError(lastException, evnt);
            }

            await _next(environment);
        }

        private static async Task Execute(object evnt, IDictionary<string, object> metaData, IDictionary<string, object> environment)
        {
            var baseTypes = FindInheritedEventTypes(evnt).Distinct().ToList();

            var subscribers = baseTypes.SelectMany(x => environment.ResolveAll(typeof(ISubscribeTo<>).MakeGenericType(x))
                            .Select(y => new { Type = x, Subscriber = y })
                            .ToList())
                            .ToList();

            //TODO:Refactor
            foreach (var subscriber in subscribers)
            {
                await ((Task)subscriber.Subscriber.GetType()
                    .GetMethod("Handle", new[] { subscriber.Type, typeof(IDictionary<string, object>) })
                    .Invoke(subscriber.Subscriber, new[] { evnt, metaData }));
            }
        }

        private static IEnumerable<Type> FindInheritedEventTypes(object evnt)
        {
            yield return evnt.GetType();

            foreach (var @interface in evnt.GetType().GetInterfaces())
                yield return @interface;

            var baseType = evnt.GetType().BaseType;

            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
    }
}