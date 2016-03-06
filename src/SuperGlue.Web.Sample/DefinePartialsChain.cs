using System.Collections.Generic;
using SuperGlue.Configuration;
using SuperGlue.FeatureToggler;
using SuperGlue.Security.Authorization;
using SuperGlue.Web.Endpoints;
using SuperGlue.Web.Output;
using SuperGlue.Web.Routing.Superscribe;

namespace SuperGlue.Web.Sample
{
    public class DefinePartialsChain : IDefineChain
    {
        public string Name => "chains.Partials";

        public void Define(IBuildAppFunction app)
        {
            app
                .Use<RouteUsingSuperscribe>()
                .Use<EnsureFeaturesAreEnabled>(new EnsureFeaturesAreEnabledSettings(async x =>
                {
                    var result = new List<IBelongToFeatures>();

                    foreach (var inputType in x.GetRouteInformation().InputTypes)
                    {
                        var belongToFeature = (await x.Bind(inputType).ConfigureAwait(false)) as IBelongToFeatures;

                        if (belongToFeature != null)
                            result.Add(belongToFeature);
                    }

                    return result;
                }))
                .Use<AuthorizeRequest>()
                .Use<ExecuteEndpoint>()
                .Use<RenderOutput>()
                .Use<WriteToOutput>();
        }

        public void AlterSettings(ChainSettings settings)
        {

        }
    }
}