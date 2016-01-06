namespace JaJo.Migrations.Storage.SuperGlue.RavenDb
{
    public class MigrationRavenStorageSettings
    {
        private string _database;

        public void UseDatabase(string database)
        {
            _database = database;
        }

        public string GetDatabase()
        {
            return string.IsNullOrEmpty(_database) ? "Migrations" : _database;
        }
    }
}