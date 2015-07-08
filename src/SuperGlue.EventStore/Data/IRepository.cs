using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.EventStore.Data
{
    public interface IRepository
    {
        Task<T> Load<T>(string id) where T : IAggregate, new();
        Task<T> LoadVersion<T>(string id, int version) where T : IAggregate, new();
        Task<IEnumerable<object>> LoadStream(string stream);
        Task RequestTimeOut(string stream, Guid commitId, object evnt, IReadOnlyDictionary<string, object> metaData, DateTime at);
        Task SaveChanges();
        Task SaveToStream(string stream, IEnumerable<object> events, Guid commitId);
        void Attache(IAggregate aggregate);

        event Action<IAggregate> AggregateLoaded;
    }
}