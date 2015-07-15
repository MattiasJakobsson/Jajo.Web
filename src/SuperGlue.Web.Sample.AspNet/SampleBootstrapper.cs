using System.Threading.Tasks;
using SuperGlue.Configuration;
using SuperGlue.Diagnostics;
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

namespace SuperGlue.Web.Sample.AspNet
{
    public class SampleBootstrapper : SuperGlueBootstrapper
    {
        protected override async Task Configure(string environment)
        {
            await AddChain("chains.Partials", app =>
            {
                app
                    .Use<RouteUsingSuperscribe>()
                    .Use<EnsureFeaturesAreEnabled>(new EnsureFeaturesAreEnabledSettings(x => x.GetRouteInformation().InputTypes))
                    .Use<AuthorizeRequest>()
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>();
            });

            await AddChain("chains.Web", app =>
            {
                app.Use<NestedStructureMapContainer>()
                    .Use<Diagnose>(new DiagnoseOptions("Urls", x => x.GetRequest().Uri.ToString()))
                    .If(x => x.GetRequest().Uri.ToString() != x.GetRequest().Uri.ToString().ToLower(), x => x.Use<RedirectTo>(new RedirectToOptions(y => y.GetRequest().Uri.ToString().ToLower())))
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
                    .Use<HandleUnitOfWork>()
                    .Use<EnsureFeaturesAreEnabled>(new EnsureFeaturesAreEnabledSettings(x => x.GetRouteInformation().InputTypes))
                    .Use<AuthorizeRequest>()
                    .Use<ValidateRequest>()
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>();
            });
        }

        protected override string ApplicationName
        {
            get { return "AspNetSample"; }
        }
    }
}