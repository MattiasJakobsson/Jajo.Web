namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ProcessManagerEventNumber
    {
        public string Id { get; set; }
        public int? LastEvent { get; set; }

        public static string GetId(string processManager)
        {
            return string.Format("ProcessManagerEventNumber/{0}", processManager);
        }
    }
}