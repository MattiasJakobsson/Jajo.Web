using System;

namespace SuperGlue.EventStore
{
    public class MessageProcessor
    {
        public event Action<DeSerializationResult> MessageArrived;

        public virtual void OnMessageArrived(DeSerializationResult obj)
        {
            var handler = MessageArrived;
            if (handler != null) handler(obj);
        }
    }
}