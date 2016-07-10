using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

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
                throw new ArgumentNullException(nameof(next));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _next = next;
            _settings = settings;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var featuresChecker = environment.Resolve<ICheckIfFeatureIsEnabled>();

            var features = (await _settings
                .GetInputsToCheck(environment).ConfigureAwait(false))
                .SelectMany(x => x.GetFeatures())
                .ToList();

            foreach (var feature in features)
            {
                var currentFeatureType = feature;

                if (await featuresChecker.IsEnabled(currentFeatureType, environment).ConfigureAwait(false)) continue;

                environment[FeatureEnvironmentExtensions.FeatureConstants.FeatureValidationFailed] = true;
                return;
            }

            await _next(environment).ConfigureAwait(false);
        }
    }
}