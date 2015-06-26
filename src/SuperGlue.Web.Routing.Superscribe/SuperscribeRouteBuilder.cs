using System;
using System.Collections.Generic;
using System.Linq;
using Superscribe.Models;
using Superscribe.Utils;
using String = Superscribe.Models.String;

namespace SuperGlue.Web.Routing.Superscribe
{
    public class SuperscribeRouteBuilder : IRouteBuilder
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

        private readonly IEnumerable<ICheckIfRouteExists> _checkIfRouteExists;
        private readonly IStringRouteParser _stringRouteParser;

        public SuperscribeRouteBuilder(IEnumerable<ICheckIfRouteExists> checkIfRouteExists, IStringRouteParser stringRouteParser)
        {
            _checkIfRouteExists = checkIfRouteExists;
            _stringRouteParser = stringRouteParser;
        }

        public void RestrictMethods(params string[] methods)
        {
            _selectedMethods = methods;
        }

        public void Append(string segment)
        {
            _node = _node == null ? new ConstantNode(segment.ToLower()) : _node.Slash(new ConstantNode(segment.ToLower()));
        }

        public void AppendParameter(RouteParameter parameter)
        {
            if (!ParamNodeFactories.ContainsKey(parameter.Type))
                return;

            var parameterNode = ParamNodeFactories[parameter.Type](parameter.Name);

            _node = _node == null ? parameterNode : _node.Slash(parameterNode);
        }

        public void AppendPattern(string pattern)
        {
            var patternNode = _stringRouteParser.MapToGraph(pattern);

            _node = _node == null ? patternNode : _node.Slash(patternNode);
        }

        public void Build(object routeTo, IDictionary<Type, Func<object, IDictionary<string, object>>> routedInputs, IDictionary<string, object> environment)
        {
            var baseNode = environment.GetRouteEngine().Base;

            var finalFunctions = _selectedMethods.Select(x => new FinalFunction(x, y =>
            {
                return _checkIfRouteExists.All(z => z.Exists(routeTo, y.Environment).Result) ? routeTo : null;
            })).ToList();

            if (_node == null)
            {
                baseNode.FinalFunctions.AddRange(finalFunctions);
                return;
            }

            _node.FinalFunctions.AddRange(finalFunctions);

            baseNode.Zip(_node.Base());

            environment.AddRouteToEndpoint(routeTo, routedInputs, _node);
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