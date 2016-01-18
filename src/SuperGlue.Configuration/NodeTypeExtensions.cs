using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    public static class NodeTypeExtensions
    {
        public class NodeTypeConstants
        {
            public const string NodeTypes = "superglue.NodeTypes";
            public const string NodeTypeChanges = "superglue.NodeTypeChanges";
            public const string NodeTypeChangeListeners = "superglue.NodeTypeChangeListeners";
            public const string ListenersStarted = "superglue.ListenersStarted";
        }

        internal static void InitializeNodeTypes(this IDictionary<string, object> environment, params string[] nodeTypes)
        {
            environment[NodeTypeConstants.NodeTypes] = new ConcurrentDictionary<string, bool>(nodeTypes.Distinct().ToDictionary(x => x, x => true));
            environment[NodeTypeConstants.NodeTypeChanges] = new ConcurrentQueue<NodeTypeChange>();
            environment[NodeTypeConstants.NodeTypeChangeListeners] = new ConcurrentDictionary<Guid, Func<string, NodeChangeType, IEnumerable<string>, Task>>();
        }

        internal static Task StartNodeChangeListeners(this IDictionary<string, object> environment)
        {
            environment[NodeTypeConstants.ListenersStarted] = true;

            return HandleQueue(environment);
        }

        internal static void StopNodeChangeListeners(this IDictionary<string, object> environment)
        {
            environment[NodeTypeConstants.ListenersStarted] = false;
        }

        public static Guid ListenToNodeTypeChanges(this IDictionary<string, object> environment, Func<string, NodeChangeType, IEnumerable<string>, Task> listener)
        {
            var id = Guid.NewGuid();
            var subscribers = environment.Get(NodeTypeConstants.NodeTypeChangeListeners, new ConcurrentDictionary<Guid, Func<string, NodeChangeType, IEnumerable<string>, Task>>());

            subscribers[id] = listener;

            return id;
        }

        public static Task AddNodeType(this IDictionary<string, object> environment, string type)
        {
            var nodeTypes = environment.Get(NodeTypeConstants.NodeTypes, new ConcurrentDictionary<string, bool>());

            if (nodeTypes.ContainsKey(type))
                return Task.CompletedTask;

            nodeTypes[type] = true;
            environment.Get(NodeTypeConstants.NodeTypeChanges, new ConcurrentQueue<NodeTypeChange>()).Enqueue(new NodeTypeChange(type, NodeChangeType.Added, new List<string>(nodeTypes.Keys)));

            return HandleQueue(environment);
        }

        public static IEnumerable<string> GetCurrentNodeTypes(this IDictionary<string, object> environment)
        {
            return new List<string>(environment.Get(NodeTypeConstants.NodeTypes, new ConcurrentDictionary<string, bool>()).Keys);
        }

        public static Task RemoveNodeType(this IDictionary<string, object> environment, string type)
        {
            var nodeTypes = environment.Get(NodeTypeConstants.NodeTypes, new ConcurrentDictionary<string, bool>());

            bool value;
            if (!nodeTypes.TryRemove(type, out value))
                return Task.CompletedTask;

            environment.Get(NodeTypeConstants.NodeTypeChanges, new ConcurrentQueue<NodeTypeChange>()).Enqueue(new NodeTypeChange(type, NodeChangeType.Removed, new List<string>(nodeTypes.Keys)));

            return HandleQueue(environment);
        }

        private static async Task HandleQueue(IDictionary<string, object> environment)
        {
            if (!environment.Get(NodeTypeConstants.ListenersStarted, false))
                return;

            var listeners = environment.Get(NodeTypeConstants.NodeTypeChangeListeners, new ConcurrentDictionary<Guid, Func<string, NodeChangeType, IEnumerable<string>, Task>>());
            var queue = environment.Get(NodeTypeConstants.NodeTypeChanges, new ConcurrentQueue<NodeTypeChange>());

            NodeTypeChange change;
            while (queue.TryDequeue(out change))
            {
                foreach (var listener in listeners)
                {
                    await listener.Value(change.Type, change.ChangeType, change.CurrentNodeTypes).ConfigureAwait(false);
                }
            }
        }

        private class NodeTypeChange
        {
            public NodeTypeChange(string type, NodeChangeType changeType, IEnumerable<string> currentNodeTypes)
            {
                Type = type;
                ChangeType = changeType;
                CurrentNodeTypes = currentNodeTypes;
            }

            public string Type { get; }
            public NodeChangeType ChangeType { get; }
            public IEnumerable<string> CurrentNodeTypes { get; }
        }
    }
}