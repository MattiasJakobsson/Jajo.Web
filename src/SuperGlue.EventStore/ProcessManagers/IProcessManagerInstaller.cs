using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace SuperGlue.EventStore.ProcessManagers
{
    public interface IProcessManagerInstaller
    {
        Task InstallProjectionFor<TProcessManager>() where TProcessManager : IManageProcess;
        Task InstallConsumerGroupFor<TProcessManager>(Func<PersistentSubscriptionSettingsBuilder, PersistentSubscriptionSettingsBuilder> alterSettings = null) where TProcessManager : IManageProcess;
    }
}