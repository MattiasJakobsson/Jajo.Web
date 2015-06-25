namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class ProjectionEventNumber
    {
        public string Id { get; set; }
        public int? LastEvent { get; set; }

        public static string GetId(string projectionName)
        {
            return string.Format("ProjectionEventNumbers/{0}", projectionName);
        }
    }
}