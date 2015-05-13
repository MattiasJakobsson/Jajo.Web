using System.Collections.Generic;

namespace SuperGlue.EventStore.Subscribers
{
    public interface ISubscribeTo<in TEvent>
    {
        void Handle(TEvent evnt, IDictionary<string, object> metaData);
    }
}