using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public class DefaultApplicationEvents : IApplicationEvents
    {
        private readonly ConcurrentDictionary<Guid, IEventHandler> _subscriptions = new ConcurrentDictionary<Guid, IEventHandler>();
        private readonly AutoResetEvent _msgAddEvent = new AutoResetEvent(false);
        private readonly CancellationTokenSource _tokenSource;
        private readonly ConcurrentQueue<object> _publishQueue = new ConcurrentQueue<object>();
        private volatile bool _starving;

        public DefaultApplicationEvents(CancellationTokenSource tokenSource)
        {
            _tokenSource = tokenSource;

            Start();
        }

        public Guid Subscribe<TEvent>(Func<TEvent, Task> handle)
        {
            var id = Guid.NewGuid();

            _subscriptions[id] = new EventHandler<TEvent>(handle);

            return id;
        }

        public bool Unsubscribe(Guid subscriptionId)
        {
            IEventHandler eventHandler;
            return _subscriptions.TryRemove(subscriptionId, out eventHandler);
        }

        public void Publish<TEvent>(TEvent evnt)
        {
            _publishQueue.Enqueue(evnt);

            if (_starving)
                _msgAddEvent.Set();
        }

        private void Start()
        {
            var token = _tokenSource.Token;
            Task.Run(async () => await HandleQueue(token).ConfigureAwait(false), token)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);

                    Start();
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task HandleQueue(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                object message;

                if (_publishQueue.TryDequeue(out message))
                {
                    var handlers = _subscriptions.Select(x => x.Value).Where(x => x.Matches(message)).ToList();

                    await Task.WhenAll(handlers.Select(x => x.Handle(message))).ConfigureAwait(false);
                }
                else
                {
                    _starving = true;
                    _msgAddEvent.WaitOne(100);
                    _starving = false;
                }
            }
        }

        private interface IEventHandler
        {
            bool Matches(object evnt);
            Task Handle(object evnt);
        }

        private class EventHandler<TEvent> : IEventHandler
        {
            private readonly Func<TEvent, Task> _handler;

            public EventHandler(Func<TEvent, Task> handler)
            {
                _handler = handler;
            }

            public bool Matches(object evnt)
            {
                return evnt != null && typeof (TEvent) == evnt.GetType();
            }

            public Task Handle(object evnt)
            {
                return _handler((TEvent)evnt);
            }
        }
    }
}