using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.UnitOfWork;

namespace JaJo.Migrations.SuperGlue
{
    public class RunMigrations : IApplicationTask
    {
        private readonly IDictionary<string, object> _environment;
        private readonly IMigrateApplication _migrateApplication;

        public RunMigrations(IDictionary<string, object> environment, IMigrateApplication migrateApplication)
        {
            _environment = environment;
            _migrateApplication = migrateApplication;
        }

        public async Task Start()
        {
            var settings = _environment.GetSettings<JaJoMigrationsSettings>() ?? new JaJoMigrationsSettings();

            foreach (var context in settings.FindContexts())
                await _migrateApplication.MigrateApplicationToLatestVersion(context);
        }

        public Task ShutDown()
        {
            return Task.CompletedTask;
        }

        public Task Exception(Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}