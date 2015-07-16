using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue
{
    internal static class DiagnosticsExtensions
    {
        public class DiagnosticsConstants
        {
            public static string AddData = "superglue.Diagnostics.AddData";
        }

        public static Task PushDiagnosticsData(this IDictionary<string, object> environment, string type, Tuple<string, IDictionary<string, object>> data)
        {
            return environment.Get(DiagnosticsConstants.AddData, (Func<IDictionary<string, object>,  string, Tuple<string, IDictionary<string, object>>, Task>)((x, y, z) => Task.CompletedTask))(environment, type, data);
        }
    }

    public static class DiagnosticTypes
    {
        public static string MiddleWareExecutionFor(IDictionary<string, object> environment)
        {
            return string.Format("{0}-middlewareexecution", environment.GetCurrentChain().Name);
        }

        public const string Setup = "Setup";
    }
}