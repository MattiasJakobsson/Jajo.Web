namespace SuperGlue.EventStore.StreamManagers.RavenDb
{
    public class RavenStreamManagerSettings
    {
        public string DatabaseName { get; private set; }

        public RavenStreamManagerSettings UsingDatabase(string dataBase)
        {
            DatabaseName = dataBase;

            return this;
        }
    }
}