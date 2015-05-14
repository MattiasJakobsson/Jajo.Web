using System;
using System.Collections.Generic;

namespace SuperGlue.Configuration
{
    public class ApplicationStartersOverrides
    {
        private readonly List<Type> _preferedApplicationStarters = new List<Type>();

        private ApplicationStartersOverrides(params Type[] prefered)
        {
            _preferedApplicationStarters.AddRange(prefered);
        }

        public static ApplicationStartersOverrides Empty()
        {
            return new ApplicationStartersOverrides();
        }

        public static ApplicationStartersOverrides Prefer<TApplicationStarter>() where TApplicationStarter : IStartApplication
        {
            return new ApplicationStartersOverrides(typeof(TApplicationStarter));
        }

        public ApplicationStartersOverrides And<TApplicationStarter>() where TApplicationStarter : IStartApplication
        {
            _preferedApplicationStarters.Add(typeof(TApplicationStarter));

            return this;
        }

        internal int GetSortOrder(IStartApplication applicationStarter)
        {
            return _preferedApplicationStarters.Contains(applicationStarter.GetType()) ? 0 : 1;
        }
    }
}