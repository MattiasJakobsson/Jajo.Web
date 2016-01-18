using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SuperGlue.Configuration
{
    public class NodeTypeRequirements
    {
        private readonly ICollection<string> _requiredNodeTypes = new Collection<string>();
        private readonly ICollection<string> _rejectedNodeTypes = new Collection<string>();

        public NodeTypeRequirements RequireNodeType(string type)
        {
            if(!_requiredNodeTypes.Contains(type))
                _requiredNodeTypes.Add(type);

            if (_rejectedNodeTypes.Contains(type))
                _rejectedNodeTypes.Remove(type);

            return this;
        }

        public NodeTypeRequirements RejectNodeType(string type)
        {
            if (!_rejectedNodeTypes.Contains(type))
                _rejectedNodeTypes.Add(type);

            if (_requiredNodeTypes.Contains(type))
                _requiredNodeTypes.Remove(type);

            return this;
        }

        internal bool IsValid(IEnumerable<string> nodeTypes)
        {
            var nodeTypeList = nodeTypes.ToList();

            return !_rejectedNodeTypes.Any(nodeTypeList.Contains) && _requiredNodeTypes.All(nodeTypeList.Contains);
        }
    }
}