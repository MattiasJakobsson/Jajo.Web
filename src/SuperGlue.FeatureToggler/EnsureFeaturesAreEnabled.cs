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

            //TODO:Use static caching so we don't have to use reflection every time
            var featureTypes = _settings
                .GetInputsToCheck(environment)
                .SelectMany(x => x.GetInterfaces())
                .Where(x => x.GenericTypeArguments.Length == 1 && typeof(IFeature).IsAssignableFrom(x.GenericTypeArguments[0]) && typeof(IBelongToFeature<>).MakeGenericType(x.GenericTypeArguments[0]) == x)
                .Select(x => x.GenericTypeArguments[0])
                .ToList();

            foreach (var feature in featureTypes)
            {
                var currentFeatureType = feature;

                while (currentFeatureType != null && typeof(IFeature).IsAssignableFrom(currentFeatureType))
                {
                    if (!await featuresChecker.IsEnabled(currentFeatureType, environment))
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
        public EnsureFeaturesAreEnabledSettings(Func<IDictionary<string, object>, IEnumerable<Type>> getInputsToCheck)
        {
            GetInputsToCheck = getInputsToCheck;
        }

        public Func<IDictionary<string, object>, IEnumerable<Type>> GetInputsToCheck { get; private set; }
    }
}