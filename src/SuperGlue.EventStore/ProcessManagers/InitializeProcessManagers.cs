using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using SuperGlue.Configuration;
using SuperGlue.EventStore.Messages;

namespace SuperGlue.EventStore.ProcessManagers
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class InitializeProcessManagers : IStartApplication
    {
        private readonly IHandleEventSerialization _eventSerialization;
        private readonly IEnumerable<IManageProcess> _processManagers;
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly IWriteToErrorStream _writeToErrorStream;
        private static bool running;
        private readonly IDictionary<string, ProcessManagerSubscription> _processManagerSubscriptions = new Dictionary<string, ProcessManagerSubscription>();

        private readonly int _dispatchWaitSeconds;
        private readonly int _numberOfEventsPerBatch;

        public InitializeProcessManagers(IHandleEventSerialization eventSerialization, IEnumerable<IManageProcess> processManagers, IWriteToErrorStream writeToErrorStream,
            IEventStoreConnection eventStoreConnection)
        {
            _eventSerialization = eventSerialization;
            _processManagers = processManagers;
            _writeToErrorStream = writeToErrorStream;
            _eventStoreConnection = eventStoreConnection;

            _dispatchWaitSeconds = 1;

            int dispatchWaitSeconds;
            if (int.TryParse(ConfigurationManager.AppSettings["BatchDispatcher.WaitSeconds"] ?? "", out dispatchWaitSeconds))
                _dispatchWaitSeconds = dispatchWaitSeconds;

            _numberOfEventsPerBatch = 256;

            int numberOfEventsPerBatch;
            if (int.TryParse(ConfigurationManager.AppSettings["BatchDispatcher.NumberOfEventsPerBatch"] ?? "", out numberOfEventsPerBatch))
                _numberOfEventsPerBatch = numberOfEventsPerBatch;
        }

        public string Chain { get { return "chains.ProcessManagers"; } }

        public async Task Start(AppFunc chain, IDictionary<string, object> settings, string environment)
        {
            running = true;

            foreach (var processManager in _processManagers)
                await SubscribeProcessManager(chain, processManager, settings);
        }

        public Task ShutDown(IDictionary<string, object> settings)
        {
            return Task.Factory.StartNew(() =>
            {
                running = false;

                foreach (var subscription in _processManagerSubscriptions)
                    subscription.Value.Close();

                _processManagerSubscriptions.Clear();
            });
        }

        public AppFunc GetDefaultChain(IBuildAppFunction buildApp, string environment)
        {
            return buildApp.Use<ExecuteProcessManager>().Build();
        }

        protected virtual void OnProcessManagerError(IManageProcess processManager, object message, IDictionary<string, object> metaData, Exception exception)
        {
            _writeToErrorStream.Write(new ProcessManagerFailed(processManager.ProcessName, exception, message, metaData), _eventStoreConnection, ConfigurationManager.AppSettings["Error.Stream"]);
        }

        private async Task SubscribeProcessManager(AppFunc chain, IManageProcess currentProcessManager, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            while (true)
            {
                if (_processManagerSubscriptions.ContainsKey(currentProcessManager.ProcessName))
                {
                    _processManagerSubscriptions[currentProcessManager.ProcessName].Close();
                    _processManagerSubscriptions.Remove(currentProcessManager.ProcessName);
                }

                try
                {
                    var eventNumberManager = environment.Resolve<IManageProcessManagerStreamEventNumbers>();

                    var messageProcessor = new MessageProcessor();
                    var messageSubscription = Observable
                        .FromEvent<DeSerializationResult>(x => messageProcessor.MessageArrived += x, x => messageProcessor.MessageArrived -= x)
                        .Buffer(TimeSpan.FromSeconds(_dispatchWaitSeconds), _numberOfEventsPerBatch)
                        .Subscribe(x => PushEventsToProcessManager(chain, currentProcessManager, x, environment));

                    var eventStoreSubscription = _eventStoreConnection.SubscribeToStreamFrom(currentProcessManager.ProcessName, await eventNumberManager.GetLastEvent(currentProcessManager.ProcessName), true,
                        (subscription, evnt) => messageProcessor.OnMessageArrived(_eventSerialization.DeSerialize(evnt.Event.EventId, evnt.Event.EventNumber, evnt.OriginalEventNumber, evnt.Event.Metadata, evnt.Event.Data)),
                        subscriptionDropped: async (subscription, reason, exception) => await SubscriptionDropped(chain, currentProcessManager, reason, exception, environment));

                    _processManagerSubscriptions[currentProcessManager.ProcessName] = new ProcessManagerSubscription(messageSubscription, eventStoreSubscription);

                    return;
                }
                catch (Exception ex)
                {
                    if (!running)
                        return;

                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    //TODO:Log
                }
            }
        }

        private async Task SubscriptionDropped(AppFunc chain, IManageProcess processManager, SubscriptionDropReason reason, Exception exception, IDictionary<string, object> environment)
        {
            if (!running)
                return;

            //TODO:Log
            await SubscribeProcessManager(chain, processManager, environment);
        }

        private void PushEventsToProcessManager(AppFunc chain, IManageProcess processManager, IEnumerable<DeSerializationResult> events, IDictionary<string, object> environment)
        {
            var eventsList = events.ToList();

            var failedEvents = eventsList.Where(x => !x.Successful);
            foreach (var evnt in failedEvents)
                OnProcessManagerError(processManager, evnt.Data, evnt.Metadata, evnt.Error);

            var successfullEvents = eventsList
                .Where(x => x.Successful);

            try
            {
                PushEvents(chain, processManager, successfullEvents, environment);
            }
            catch (Exception ex)
            {
                //TODO:Log
            }
        }

        private void PushEvents(AppFunc chain, IManageProcess processManager, IEnumerable<DeSerializationResult> events, IDictionary<string, object> environment)
        {
            var requestEnvironment = new Dictionary<string, object>();
            foreach (var item in environment)
                requestEnvironment[item.Key] = item.Value;

            var request = requestEnvironment.GetEventStoreRequest();

            request.ProcessManager = processManager;
            request.Events = events;
            request.OnException = (exception, evnt) => OnProcessManagerError(processManager, evnt.Data, evnt.Metadata, exception);

            chain(requestEnvironment);
        }
    }
}