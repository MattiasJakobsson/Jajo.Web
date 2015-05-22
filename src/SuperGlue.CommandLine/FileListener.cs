using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace SuperGlue
{
    public class FileListener : IFileListener
    {
        private readonly IDictionary<string, FileSystemWatcher> _fileSystemWatchers = new ConcurrentDictionary<string, FileSystemWatcher>();

        public void StartListening(string directory, string filter, Action<string> directoryChanged)
        {
            var key = string.Concat(directory, filter);

            if (_fileSystemWatchers.ContainsKey(key))
            {
                _fileSystemWatchers[key].Dispose();
                _fileSystemWatchers.Remove(key);
            }

            var fileSystemWatcher = new FileSystemWatcher(directory, filter)
            {
                EnableRaisingEvents = true
            };

            fileSystemWatcher.Created += (sender, eventArgs) =>
            {
                directoryChanged(eventArgs.FullPath);
            };

            fileSystemWatcher.Changed += (sender, eventArgs) =>
            {
                directoryChanged(eventArgs.FullPath);
            };

            fileSystemWatcher.Deleted += (sender, eventArgs) =>
            {
                directoryChanged(eventArgs.FullPath);
            };
            
            _fileSystemWatchers[key] = fileSystemWatcher;
        }

        public void StopListeners()
        {
            foreach (var fileSystemWatcher in _fileSystemWatchers)
                fileSystemWatcher.Value.Dispose();

            _fileSystemWatchers.Clear();
        }
    }
}