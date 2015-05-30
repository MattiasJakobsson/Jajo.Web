using SuperGlue.Configuration;
using SuperGlue.Diagnostics;
using SuperGlue.ExceptionManagement;
using SuperGlue.RequestBranching;
using SuperGlue.Security.Authorization;
using SuperGlue.StructureMap;
using SuperGlue.UnitOfWork;
using SuperGlue.Web.Endpoints;
using SuperGlue.Web.Http;
using SuperGlue.Web.ModelBinding;
using SuperGlue.Web.Output;
using SuperGlue.Web.Output.Spark;
using SuperGlue.Web.Routing.Superscribe;
using SuperGlue.Web.Routing.Superscribe.Policies.MethodEndpoint;
using SuperGlue.Web.Validation;
using SuperGlue.Web.Validation.InputValidation;

namespace SuperGlue.Web.Sample
{
    public class SampleBootstrapper : SuperGlueBootstrapper
    {
        protected override void Configure()
        {
            this.UseRoutePolicy(new MethodEndpointRoutePolicy(new DefaultEndpointBuilder()));

            this.ConfigureOutput()
                .When(x => x.GetRequest().Headers.Accept.Contains("text/html")).UseSpark()
                .When(x => true).UseRenderer(new RenderOutputAsJson());

            AddChain("chains.Partials", app =>
            {
                app
                    .Use<HandleExceptions>()
                    .Use<AuthorizeRequest>(new AuthorizeRequestOptions().WithAuthorizer(new TestAuthorizer()))
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>();
            });

            AddChain("chains.Web", app =>
            {
                app.Use<NestedStructureMapContainer>()
                    .Use<Diagnose>(new DiagnoseOptions("Urls", x => x.GetRequest().Uri.ToString()))
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
                        .AddCase(y => y.GetRouteInformation().RoutedTo == null, app
                            .New()
                            .Use<SetStatusCode>(404)
                            .Use<HandleNotFoundMiddleware>()
                            .Build()))
                    .Use<HandleExceptions>()
                    .Use<BindModels>()
                    .Use<RouteUsingSuperscribe>()
                    .Use<HandleUnitOfWork>()
                    .Use<AuthorizeRequest>(new AuthorizeRequestOptions().WithAuthorizer(new TestAuthorizer()))
                    .Use<ValidateRequest>(new ValidateRequestOptions().UsingValidator(new ValidateRequestInput()))
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>();
            });
        }
    }
}