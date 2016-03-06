using System.Collections.Generic;
using SuperGlue.Configuration;
using SuperGlue.ExceptionManagement;
using SuperGlue.FeatureToggler;
using SuperGlue.RequestBranching;
using SuperGlue.Security.Authorization;
using SuperGlue.StructureMap;
using SuperGlue.UnitOfWork;
using SuperGlue.Web.Endpoints;
using SuperGlue.Web.Http;
using SuperGlue.Web.ModelBinding;
using SuperGlue.Web.Output;
using SuperGlue.Web.Routing.Superscribe;
using SuperGlue.Web.Validation;

namespace SuperGlue.Web.Sample
{
    public class DefineWebChain : IDefineChain
    {
        public string Name => "chains.Web";

        public void Define(IBuildAppFunction app)
        {
            app.Use<GzipOutput>()
                .Use<NestedStructureMapContainer>()
                .If(x => x.GetRequest().Uri.ToString() != x.GetRequest().Uri.ToString().ToLower(),
                    x => x.Use<RedirectTo>(new RedirectToOptions(y => y.GetRequest().Uri.ToString().ToLower())))
                .Use<BranchRequest>(new BranchRequestConfiguration()
                    .AddCase(y => y.GetException() != null, app
                        .New()
                        .Use<SetStatusCode>(500)
                        .Use<RollbackUnitOfWork>()
                        .Use<HandledExceptionMiddleware>()
                        .Build())
                    .AddCase(y => y.HasAuthorizationFailed(), app
                        .New()
                        .Use<SetStatusCode>(401)
                        .Use<HandleUnauthorizedMiddleware>()
                        .Build())
                    .AddCase(y => !y.GetValidationResult().IsValid, app
                        .New()
                        .Use<HandleValidationErrorMiddleware>()
                        .Build())
                    .AddCase(y => y.GetRouteInformation().RoutedTo == null || y.HasFeatureValidationFailed(), app
                        .New()
                        .Use<SetStatusCode>(404)
                        .Use<HandleNotFoundMiddleware>()
                        .Build()))
                .Use<HandleExceptions>()
                .Use<BindModels>()
                .Use<RouteUsingSuperscribe>()
                .Use<HandleUnitOfWork>(new HandleUnitOfWorkOptions())
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
                .Use<ValidateRequest>()
                .Use<ExecuteEndpoint>()
                .Use<RenderOutput>()
                .Use<WriteToOutput>();
        }

        public void AlterSettings(ChainSettings settings)
        {
            settings.SetWebApplicationRoot("/test/");
        }
    }
}