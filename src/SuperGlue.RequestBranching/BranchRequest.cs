using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SuperGlue.RequestBranching
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class BranchRequest
    {
        private readonly AppFunc _next;
        private readonly BranchRequestConfiguration _configuration;

        public BranchRequest(AppFunc next, BranchRequestConfiguration configuration)
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

    public class BranchRequestConfiguration
    {
        private readonly IList<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>> _cases = new List<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>>();

        public IReadOnlyCollection<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>> Cases { get { return new ReadOnlyCollection<Tuple<Func<IDictionary<string, object>, bool>, AppFunc>>(_cases); } }

        public BranchRequestConfiguration AddCase(Func<IDictionary<string, object>, bool> check, AppFunc result)
        {
            _cases.Add(new Tuple<Func<IDictionary<string, object>, bool>, AppFunc>(check, result));

            return this;
        }
    }
}