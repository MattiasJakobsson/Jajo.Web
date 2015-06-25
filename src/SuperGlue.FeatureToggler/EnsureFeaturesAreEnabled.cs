using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperGlue.FeatureToggler
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class EnsureFeaturesAreEnabled
    {
        private readonly AppFunc _next;
        private readonly EnsureFeaturesAreEnabledSettings _settings;

        public EnsureFeaturesAreEnabled(AppFunc next, EnsureFeaturesAreEnabledSettings settings)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (_settings == null)
                throw new ArgumentNullException("_settings");

            _next = next;
            _settings = settings;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var featuresChecker = environment.Resolve<ICheckIfFeatureIsEnabled>();

            foreach (var feature in _settings.GetFeaturesFromRequest(environment).Where(x => x != null))
            {
                var currentFeatureType = feature;

                while (currentFeatureType != null && typeof(IFeature).IsAssignableFrom(currentFeatureType))
                {
                    if (!featuresChecker.IsEnabled(currentFeatureType, environment))
                    {
                        environment[FeatureEnvironmentExtensions.FeatureConstants.FeatureValidationFailed] = true;
                        return;
                    }

                    currentFeatureType = currentFeatureType.BaseType;
                }
            }

            await _next(environment);
        }
    }

    public class EnsureFeaturesAreEnabledSettings
    {
        public EnsureFeaturesAreEnabledSettings(Func<IDictionary<string, object>, IEnumerable<Type>> getFeaturesFromRequest)
        {
            GetFeaturesFromRequest = getFeaturesFromRequest;
        }

        public Func<IDictionary<string, object>, IEnumerable<Type>> GetFeaturesFromRequest { get; private set; }
    }
}