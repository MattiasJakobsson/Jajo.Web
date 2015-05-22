using System;
using System.Runtime.Remoting.Channels;

namespace SuperGlue
{
    public interface IFileListener
    {
        void StartListening(string directory, string filter, Action<string> directoryChanged);
        void StopListeners();
    }
}