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
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var evnt = environment.GetEventStoreRequest().Event;

            await Execute(evnt, environment).ConfigureAwait(false);

            await _next(environment).ConfigureAwait(false);
        }

        private static async Task Execute(DeSerializationResult evnt, IDictionary<string, object> environment)
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
                    .Invoke(subscriber.Subscriber, new[] { evnt.Data, evnt.Metadata })).ConfigureAwait(false);
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