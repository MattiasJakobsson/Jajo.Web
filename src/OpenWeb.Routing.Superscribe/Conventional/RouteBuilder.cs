using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Superscribe.Models;
using String = Superscribe.Models.String;

namespace OpenWeb.Routing.Superscribe.Conventional
{
    public class RouteBuilder : IRouteBuilder
    {
        private static readonly IDictionary<Type, Func<string, ParamNode>> ParamNodeFactories = new Dictionary<Type, Func<string, ParamNode>>
        {
            {typeof(int), CreateIntNode},
            {typeof(long), CreateLongNode},
            {typeof(string), CreateStringNode},
            {typeof(bool), CreateBoolNood},
            {typeof(System.Guid), CreateGuidNode}
        };

        private IEnumerable<string> _selectedMethods = new List<string>
        {
            "GET",
            "POST"
        };

        private GraphNode _node;

        public void RestrictMethods(params string[] methods)
        {
            _selectedMethods = methods;
        }

        public void Append(string segment)
        {
            _node = _node == null ? new ConstantNode(segment.ToLower()) : _node.Slash(new ConstantNode(segment.ToLower()));
        }

        public void AppendParameter(PropertyInfo parameter)
        {
            if (!ParamNodeFactories.ContainsKey(parameter.PropertyType))
                return;

            var parameterNode = ParamNodeFactories[parameter.PropertyType](parameter.Name);

            _node = _node == null ? parameterNode : _node.Slash(parameterNode);
        }

        public GraphNode Build(GraphNode baseNode, MethodInfo routeTo)
        {
            var finalFunctions = _selectedMethods.Select(x => new FinalFunction(x, y => routeTo)).ToList();

            if (_node == null)
            {
                baseNode.FinalFunctions.AddRange(finalFunctions);
                return baseNode;
            }

            _node.FinalFunctions.AddRange(finalFunctions);

            baseNode.Zip(_node.Base());

            return _node;
        }

        private static ParamNode CreateIntNode(string name)
        {
            return new Int(name);
        }

        private static ParamNode CreateLongNode(string name)
        {
            return new Long(name);
        }

        private static ParamNode CreateStringNode(string name)
        {
            return new String(name);
        }

        private static ParamNode CreateBoolNood(string name)
        {
            return new Bool(name);
        }

        private static ParamNode CreateGuidNode(string name)
        {
            return new Guid(name);
        }
    }
}