using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly ICollection<IRouteConstraint> _constraints = new List<IRouteConstraint>();

        private readonly IEnumerable<ICheckIfRouteExists> _checkIfRouteExists;
        private readonly IStringRouteParser _stringRouteParser;

        public SuperscribeRouteBuilder(IEnumerable<ICheckIfRouteExists> checkIfRouteExists, IStringRouteParser stringRouteParser, IDictionary<string, object> environment)
        {
            _checkIfRouteExists = checkIfRouteExists;
            _stringRouteParser = stringRouteParser;

            foreach (var part in environment.GetWebApplicationRoot().Split('/').Where(x => !string.IsNullOrEmpty(x)))
                Append(part);
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

        public void AddConstraint(IRouteConstraint constraint)
        {
            _constraints.Add(constraint);
        }

        public Task Build(object routeTo, IDictionary<Type, Func<object, IDictionary<string, object>>> routedInputs, IDictionary<string, object> environment)
        {
            var baseNode = environment.GetRouteEngine().Base;

            var finalFunctions = _selectedMethods.Select(x => new FinalFunction(x, y =>
            {
                ((IDictionary<string, object>)y.Environment)[RouteExtensions.RouteConstants.Parameters] = (IDictionary<string, object>)y.Parameters;
                return _checkIfRouteExists.All(z => z.Exists(routeTo, y.Environment).Result) ? routeTo : null;
            })).ToList();

            if (_node == null)
            {
                baseNode.FinalFunctions.AddRange(finalFunctions);
                return Task.CompletedTask;
            }

            _node.FinalFunctions.AddRange(finalFunctions);

            var oldActivationFunction = _node.ActivationFunction;
            _node.ActivationFunction = (routeData, segment) => _constraints.All(x => x.IsValid(routeTo)) && oldActivationFunction(routeData, segment);

            baseNode.Zip(_node.Base());

            environment.AddRouteToEndpoint(routeTo, routedInputs, _node);

            environment.Log($"Created route for: {GetPattern()}", LogLevel.Debug);

            return environment.PushDiagnosticsData(DiagnosticsCategories.Setup, DiagnosticsTypes.Bootstrapping, "Routes", new Tuple<string, IDictionary<string, object>>($"Route created for {GetPattern()}", new Dictionary<string, object>
            {
                {"Pattern", GetPattern()},
                {"Inputs", string.Join(", ", routedInputs.Select(x => x.Key.Name))},
                {"RoutedTo", routeTo}
            }));
        }

        private string GetPattern()
        {
            var patternParts = new List<string>();

            var node = _node;

            while (node != null)
            {
                var paramNode = node as ParamNode;

                patternParts.Add(paramNode != null ? string.Concat("{", paramNode.Name, "}") : node.Template);

                patternParts.Add("/");

                node = node.Parent;
            }

            var patternBuilder = new StringBuilder();
            patternParts.Reverse();

            foreach (var part in patternParts)
                patternBuilder.Append(part);

            return patternBuilder.ToString();
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