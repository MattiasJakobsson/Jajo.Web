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

        protected override Task<IReadOnlyCollection<IMigration<TContext>>> FindMigrationsFor<TContext>(TContext context)
        {
            var migrations = _environment.ResolveAll(typeof(IMigration<>).MakeGenericType(context.GetType())).OfType<IMigration<TContext>>().ToList();

            return Task.FromResult<IReadOnlyCollection<IMigration<TContext>>>(new ReadOnlyCollection<IMigration<TContext>>(migrations));
        }
    }
}