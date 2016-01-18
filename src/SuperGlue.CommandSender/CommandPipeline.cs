using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.CommandSender
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class CommandPipeline
    {
        private static AppFunc currentPipeline;

        public static AppFunc CurrentPipeline
        {
            get { return currentPipeline ?? (x => Task.CompletedTask); }
        }

        public static void Use(AppFunc pipeline)
        {
            currentPipeline = pipeline;
        }
    }
}