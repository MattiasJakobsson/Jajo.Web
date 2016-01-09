using System;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IProcessManagerInstaller
    {
        void InstallProjectionFor<TProcessManager>() where TProcessManager : IManageProcess;
        void InstallConsumerGroupFor<TProcessManager>(Func<PersistentSubscriptionSettingsBuilder, PersistentSubscriptionSettingsBuilder> alterSettings = null) where TProcessManager : IManageProcess;
    }
}