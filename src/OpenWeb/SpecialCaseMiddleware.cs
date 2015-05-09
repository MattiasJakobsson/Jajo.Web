using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace OpenWeb
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SpecialCaseMiddleware
    {
        private readonly AppFunc _next;
        private readonly SpecialCaseConfiguration _configuration;

        public SpecialCaseMiddleware(AppFunc next, SpecialCaseConfiguration configuration)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            foreach (var item in _configuration.Cases)
            {
                if (item.Item1(environment))
                    await item.Item2(environment);
            }
        }
    }

    public class SpecialCaseConfiguration
    {
        private readonly IList<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>> _cases = new List<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>>();

        public IReadOnlyCollection<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>> Cases { get { return new ReadOnlyCollection<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>>(_cases); } }

        public SpecialCaseConfiguration AddCase(Func<IDictionary<string, object>, bool> check, AppFunc result)
        {
            _cases.Add(new Tuple<Func<IDictionary<string, object>, bool>, AppFunc>(check, result));

            return this;
        }
    }
}