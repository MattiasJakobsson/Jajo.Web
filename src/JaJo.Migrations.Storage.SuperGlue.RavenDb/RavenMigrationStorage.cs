using System.Collections.Generic;
using System.Threading.Tasks;
using Raven.Client;
using SuperGlue.Configuration;
using SuperGlue.RavenDb;

namespace JaJo.Migrations.Storage.SuperGlue.RavenDb
{
    public class RavenMigrationStorage : IMigrationStorage
    {
        private readonly IAsyncDocumentSession _session;

        public RavenMigrationStorage(IRavenSessions ravenSessions, IDictionary<string, object> environment)
        {
            var databaseName = (environment.GetSettings<MigrationRavenStorageSettings>() ?? new MigrationRavenStorageSettings()).GetDatabase();

            _session = ravenSessions.GetFor(databaseName);
        }

        public async Task<int> GetVersionFor(string migration)
        {
            return (await GetVersion(migration)).Version ?? 0;
        }

        public async Task SaveVersionFor(string migration, int version)
        {
            var migrationVersion = await GetVersion(migration);

            migrationVersion.Version = version;
        }

        private async Task<MigrationVersion> GetVersion(string migration)
        {
            var migrationVersion = await _session.LoadAsync<MigrationVersion>(MigrationVersion.BuildId(migration));

            if (migrationVersion != null)
                return migrationVersion;

            migrationVersion = new MigrationVersion
            {
                Id = MigrationVersion.BuildId(migration)
            };

            await _session.StoreAsync(migrationVersion);

            return migrationVersion;
        }

        public class MigrationVersion
        {
            public string Id { get; set; }
            public int? Version { get; set; }

            public static string BuildId(string migration)
            {
                return string.Format("Migrations/{0}/Version", migration);
            }
        }
    }
}