using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;

namespace OpenWeb.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class IfExtensions
    {
        public static IAppBuilder If(this IAppBuilder appBuilder, Func<IDictionary<string, object>, bool> check, Action<IAppBuilder> configure)
        {
            var options = new IfOptions(check);

            appBuilder.Use(typeof (IfMiddleware), options);

            var branch = appBuilder.New();
            configure(branch);
            options.Branch = (AppFunc) branch.Build(typeof (AppFunc));

            return appBuilder;
        }
    }
}