using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public static class BuildAppFunctionExtensions
    {
        public static IBuildAppFunction If(this IBuildAppFunction builder, Func<IDictionary<string, object>, bool> check, Action<IBuildAppFunction> chainBuilder)
        {
            var childChainBuilder = builder.New();

            chainBuilder(childChainBuilder);

            var shouldContinueKey = string.Format("superglue.ShouldContinue.{0}", Guid.NewGuid());

            childChainBuilder.Use<Continue>(new ContinueOptions(x => x[shouldContinueKey] = true));

            builder.Use<If>(new IfOptions(check, childChainBuilder.Build(), (x, y) => !y || x.Get<bool>(shouldContinueKey)));

            return builder;
        }
    }
}