using System;
using System.Collections.Generic;

namespace SuperGlue.EventStore
{
    public interface IRepository
    {
        T Load<T>(string id, ActionMetaData actionMetaData) where T : IAggregate, new();
        T LoadVersion<T>(string id, int version, ActionMetaData actionMetaData) where T : IAggregate, new();
        IEnumerable<object> LoadStream(string stream);
        void RequestTimeOut(string stream, Guid commitId, object evnt, IReadOnlyDictionary<string, object> metaData, DateTime at);
        void Save(IAggregate aggregate, Guid commitId, ActionMetaData actionMetaData);
        void SaveToStream(string stream, IEnumerable<object> events, Guid commitId, ActionMetaData actionMetaData);
        void SaveToStream(string stream, IEnumerable<object> events, Guid commitId, string context, ActionMetaData actionMetaData);
        void SaveToNamedStream(string stream, IEnumerable<object> events, Guid commitId, string context, ActionMetaData actionMetaData);
        void Attache(IAggregate aggregate, ActionMetaData actionMetaData);

        event Action<IAggregate, ActionMetaData> AggregateLoaded;
    }
}