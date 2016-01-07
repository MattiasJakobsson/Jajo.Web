using System.Collections.Generic;

namespace SuperGlue.Monitoring
{
    public static class MonitoringEnvironmentExtensions
    {
        public static class MonitoringConstants
        {
            public const string MonitorHeartBeats = "superglue.MonitorHeartBeats";
        }

        public static IMonitorHeartBeats GetHeartBeatMonitor(this IDictionary<string, object> environment)
        {
            return environment.Get<IMonitorHeartBeats>(MonitoringConstants.MonitorHeartBeats);
        }

        internal static void SetHeartBeatMonitor(this IDictionary<string, object> environment, IMonitorHeartBeats monitorHeartBeats)
        {
            environment[MonitoringConstants.MonitorHeartBeats] = monitorHeartBeats;
        }
    }
}