using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Notifications
{
    public interface IRecieveNotifications
    {
        Task ErrorNotification(string from, string message, IDictionary<string, object> environment, Exception exception = null);
    }
}