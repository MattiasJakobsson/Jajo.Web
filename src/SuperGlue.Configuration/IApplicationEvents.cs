using System;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public interface IApplicationEvents
    {
        Guid Subscribe<TEvent>(Func<TEvent, Task> handle);
        bool Unsubscribe(Guid subscriptionId);
        void Publish<TEvent>(TEvent evnt);
    }
}