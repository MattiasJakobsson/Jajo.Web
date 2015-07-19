using System.Collections.Generic;
using System.Threading.Tasks;
using Should;

namespace SuperGlue.ParameterParsing.Tests
{
    public class DefaultParameterHandlerTests
    {
        public void when_parsing_simple_parameter_correct_value_should_be_returned()
        {
            var parser = new DefaultParameterHandler(new List<IFindParameterValue>
            {
                new FakeParameterFinder(new Dictionary<string, object>
                {
                    {"name", "mattias"}
                })
            });

            var result = parser.ParseParameters("${name}", new Dictionary<string, object>()).Result;

            result.ShouldEqual("mattias");
        }

        public class FakeParameterFinder : IFindParameterValue
        {
            private readonly IDictionary<string, object> _values;

            public FakeParameterFinder(IDictionary<string, object> values)
            {
                _values = values;
            }

            public Task<object> Find(string parameter, IDictionary<string, object> environment)
            {
                return Task.FromResult(_values[parameter]);
            }
        }
    }
}