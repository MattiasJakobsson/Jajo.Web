using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Consensus.Messages;

namespace SuperGlue.Consensus
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public abstract class ConsensusAwareBootstrapper : SuperGlueBootstrapper
    {
        private readonly IDictionary<Type, NodeRole> _requiredRoles = new Dictionary<Type, NodeRole>();
        private IEnumerable<StateAwareAppStarter> _appStarters;

        protected override Task BeforeApplicationStart(IDictionary<string, object> settings, IEnumerable<IStartApplication> appStarters)
        {
            var appStarterList = appStarters.ToList();

            _appStarters = appStarterList
                .Select(x => new StateAwareAppStarter(x, _requiredRoles.ContainsKey(x.GetType()) ? (NodeRole?)_requiredRoles[x.GetType()] : null))
                .ToList();

            var applicationEvents = settings.Resolve<IApplicationEvents>();

            applicationEvents.Subscribe<NodeRoleTransitioned>(x => TransitionTo(x.NewRole));

            return base.BeforeApplicationStart(settings, appStarterList);
        }

        protected void RequireRoleFor<TStarter>(NodeRole role) where TStarter : IStartApplication
        {
            _requiredRoles[typeof(TStarter)] = role;
        }

        protected override Task RunStarters(IEnumerable<IStartApplication> appStarters)
        {
            return TransitionTo(NodeRole.Follower);
        }

        protected override Task RunShutdowns(IEnumerable<IStartApplication> appStarters)
        {
            var actions = new ConcurrentBag<Task>();

            Parallel.ForEach(appStarters, x =>
            {
                actions.Add(x.ShutDown(Environment));
            });

            return Task.WhenAll(actions);
        }

        private Task TransitionTo(NodeRole newRole)
        {
            var startTasks = new ConcurrentBag<Task>();

            Parallel.ForEach(_appStarters, starter =>
            {
                AppFunc chain = null;

                if (Chains.ContainsKey(starter.Chain))
                    chain = Chains[starter.Chain];

                chain = chain ?? starter.GetDefaultChain(GetAppFunctionBuilder(starter.Chain), Environment, ApplicationEnvironment);

                if (chain != null)
                    startTasks.Add(starter.BringToState(chain, Environment, ApplicationEnvironment, newRole));
            });

            return Task.WhenAll(startTasks);
        }
    }
}