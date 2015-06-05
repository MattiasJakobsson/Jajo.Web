using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SuperGlue.Configuration
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class BuildAppFunction : IBuildAppFunction
    {
        private readonly ICollection<MiddlewareWithArgs> _middleware = new List<MiddlewareWithArgs>();
        private readonly IDictionary<string, object> _environment;

        public BuildAppFunction(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public IBuildAppFunction Use<TMiddleware>(params object[] args)
        {
            var wrappers = _environment.ResolveAll<IWrapMiddleware<TMiddleware>>().ToList();

            if (wrappers.Any())
                _middleware.Add(new MiddlewareWithArgs(typeof(WrapMiddleware<TMiddleware>), new object[] { new WrapMiddlewareOptions<TMiddleware>(wrappers) }));

            _middleware.Add(new MiddlewareWithArgs(typeof(TMiddleware), args));

            return this;
        }

        public AppFunc Build()
        {
            var list = new List<MiddlewareWithArgs>(_middleware);

            list.Reverse();

            AppFunc lastFunc = x => Task.Factory.StartNew(() => { });

            foreach (var item in list)
            {
                var args = new List<object>
                {
                    lastFunc
                };

                args.AddRange(item.Args);

                var constructor = item.Type.GetConstructor(args.Select(x => x.GetType()).ToArray());

                if (constructor == null)
                    break;

                var instance = constructor.Invoke(args.ToArray());

                var method = instance.GetType().GetMethod("Invoke", new[] { typeof(IDictionary<string, object>) });

                var parameter = Expression.Parameter(typeof(IDictionary<string, object>));

                lastFunc = Expression.Lambda<AppFunc>(Expression.Call(Expression.Constant(instance), method, parameter), parameter).Compile();
            }

            return lastFunc;
        }

        public IBuildAppFunction New()
        {
            return new BuildAppFunction(_environment);
        }

        public class MiddlewareWithArgs
        {
            public MiddlewareWithArgs(Type type, object[] args)
            {
                Type = type;
                Args = args;
            }

            public Type Type { get; private set; }
            public object[] Args { get; private set; }
        }
    }
}