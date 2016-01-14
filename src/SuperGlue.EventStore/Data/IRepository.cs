using System;
using System.Threading.Tasks;
using SuperGlue.EventStore.ProcessManagers;
using SuperGlue.EventTracking;

namespace SuperGlue.EventStore.Data
{
    public interface IRepository
    {
        Task<T> Load<T>(string id) where T : IAggregate, new();
        Task<T> Load<T>(string streamName, string id) where T : IProcessManagerState, new();
        Task RequestTimeOut(string stream, object evnt, DateTime at);
        Task RequestTimeOut(string stream, object evnt, TimeSpan @in);
        Task SaveChanges();
        void ThrowAwayChanges();
        void Attach(IAggregate aggregate);
        void Attach(ICanApplyEvents canApplyEvents);
        void Attach(object command, Guid id, string causedBy);
    }
}