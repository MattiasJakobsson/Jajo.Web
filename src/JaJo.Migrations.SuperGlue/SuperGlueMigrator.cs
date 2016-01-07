using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue;

namespace JaJo.Migrations.SuperGlue
{
    public class SuperGlueMigrator : ApplicationMigrator
    {
        private readonly IDictionary<string, object> _environment;

        public SuperGlueMigrator(IMigrationStorage storage, IDictionary<string, object> environment) : base(storage)
        {
            _environment = environment;
        }

        protected override Task<IReadOnlyCollection<IMigrationWrapper>> FindMigrationsFor(IMigrationContext context)
        {
            var migrations = (IMigrations)_environment.Resolve(typeof(MigrationsList<>).MakeGenericType(context.GetType()));

            return Task.FromResult<IReadOnlyCollection<IMigrationWrapper>>(new ReadOnlyCollection<IMigrationWrapper>(migrations.GetMigrations(context).ToList()));
        }

        private interface IMigrations
        {
            IEnumerable<IMigrationWrapper> GetMigrations(IMigrationContext context);
        }

        private class MigrationsList<TContext> : IMigrations where TContext : IMigrationContext
        {
            private readonly IEnumerable<IMigration<TContext>> _migrations;

            public MigrationsList(IEnumerable<IMigration<TContext>> migrations)
            {
                _migrations = migrations;
            }

            public IEnumerable<IMigrationWrapper> GetMigrations(IMigrationContext context)
            {
                return _migrations.Select(x => new MigrationWrapper<TContext>(x, (TContext)context));
            }
        }
    }
}