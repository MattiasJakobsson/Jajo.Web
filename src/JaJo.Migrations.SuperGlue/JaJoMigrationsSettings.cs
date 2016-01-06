using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JaJo.Migrations.SuperGlue
{
    public class JaJoMigrationsSettings
    {
        private readonly ICollection<Func<IReadOnlyCollection<IMigrationContext>>> _findContexts = new List<Func<IReadOnlyCollection<IMigrationContext>>>();

        public void FindContextsUsing(Func<IReadOnlyCollection<IMigrationContext>> findContexts)
        {
            _findContexts.Add(findContexts);
        }

        public IReadOnlyCollection<IMigrationContext> FindContexts()
        {
            return new ReadOnlyCollection<IMigrationContext>(_findContexts.SelectMany(x => x()).ToList());
        }
    }
}