using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using SuperGlue.Configuration;
using SuperGlue.Consensus.Messages;
using SuperGlue.Discovery.Consul;
using SuperGlue.UnitOfWork;

namespace SuperGlue.Consensus.LeaderElection.Consul
{
    public class HandleLeaderElection : IApplicationTask
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private readonly IApplicationEvents _applicationEvents;
        private NodeRole _currentRole = NodeRole.Follower;

        public HandleLeaderElection(IApplicationEvents applicationEvents)
        {
            _applicationEvents = applicationEvents;
        }

        public Task Start(IDictionary<string, object> environment)
        {
            var consulServiceSettings = environment.GetSettings<ConsulServiceSettings>();

            var client = consulServiceSettings.CreateClient();

            var consulLock = client.CreateLock($"service/{consulServiceSettings.Name}/leader");

            StartLeaderElection(consulLock, CancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        public Task ShutDown(IDictionary<string, object> environment)
        {
            CancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }

        public Task Exception(IDictionary<string, object> environment, Exception exception)
        {
            return Task.CompletedTask;
        }

        private void StartLeaderElection(IDistributedLock consulLock, CancellationToken cancellationToken)
        {
            Task.Run(async () => await AcquireLock(consulLock, cancellationToken).ConfigureAwait(false), cancellationToken)
                .ContinueWith(t =>
                {
                    (t.Exception ?? new AggregateException()).Handle(ex => true);

                    StartLeaderElection(consulLock, cancellationToken);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task AcquireLock(IDistributedLock consulLock, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!consulLock.IsHeld)
                    consulLock.Acquire(cancellationToken);

                while (!cancellationToken.IsCancellationRequested && consulLock.IsHeld)
                {
                    if (_currentRole != NodeRole.Leader)
                    {
                        _applicationEvents.Publish(new NodeRoleTransitioned(NodeRole.Leader));
                        _currentRole = NodeRole.Leader;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
                }

                if (_currentRole != NodeRole.Follower && !consulLock.IsHeld)
                {
                    _applicationEvents.Publish(new NodeRoleTransitioned(NodeRole.Follower));
                    _currentRole = NodeRole.Follower;
                }
            }
        }
    }
}