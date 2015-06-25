namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ServiceEventNumber
    {
        public string Id { get; set; }
        public int? LastEvent { get; set; }

        public static string GetId(string service, string stream)
        {
            return string.Format("ServiceEventNumber/{0}/{1}", service, stream);
        }
    }
}