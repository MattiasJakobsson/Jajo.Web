using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.UnitOfWork;

namespace JaJo.Migrations.SuperGlue
{
    public class RunMigrations : IApplicationTask
    {
        private readonly IMigrateApplication _migrateApplication;

        public RunMigrations(IMigrateApplication migrateApplication)
        {
            _migrateApplication = migrateApplication;
        }

        public async Task Start(IDictionary<string, object> environment)
        {
            var settings = environment.GetSettings<JaJoMigrationsSettings>() ?? new JaJoMigrationsSettings();

            foreach (var context in settings.FindContexts())
                await _migrateApplication.MigrateApplicationToLatestVersion(context).ConfigureAwait(false);
        }

        public Task ShutDown(IDictionary<string, object> environment)
        {
            return Task.CompletedTask;
        }

        public Task Exception(IDictionary<string, object> environment, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}