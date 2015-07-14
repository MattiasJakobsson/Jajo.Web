namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ProjectionEventNumber
    {
        public string Id { get; set; }
        public int? LastEvent { get; set; }

        public static string GetId(string service, string projectionName)
        {
            return string.Format("Services/{0}/Projections/{1}", service, projectionName);
        }
    }
}