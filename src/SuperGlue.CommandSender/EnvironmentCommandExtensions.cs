using System.Collections.Generic;

namespace SuperGlue.CommandSender
{
    public static class EnvironmentCommandExtensions
    {
        public static class CommandConstants
        {
            public const string CurrentCommand = "CurrentCommand";
        }

        public static object GetCurrentCommand(this IDictionary<string, object> environment)
        {
            return environment.Get<object>(CommandConstants.CurrentCommand);
        }
    }
}