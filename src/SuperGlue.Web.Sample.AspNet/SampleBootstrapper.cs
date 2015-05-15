using System;
using System.Collections.Generic;
using System.Linq;
using SuperGlue.Configuration;
using SuperGlue.Diagnostics;
using SuperGlue.ExceptionManagement;
using SuperGlue.RequestBranching;
using SuperGlue.Security.Authorization;
using SuperGlue.StructureMap;
using SuperGlue.UnitOfWork;
using SuperGlue.Web.Diagnostics;
using SuperGlue.Web.Endpoints;
using SuperGlue.Web.Http;
using SuperGlue.Web.ModelBinding;
using SuperGlue.Web.Output;
using SuperGlue.Web.Output.Spark;
using SuperGlue.Web.Routing.Superscribe;
using SuperGlue.Web.Routing.Superscribe.Conventional;
using SuperGlue.Web.Validation;
using SuperGlue.Web.Validation.InputValidation;

namespace SuperGlue.Web.Sample.AspNet
{
    public class SampleBootstrapper : SuperGlueBootstrapper
    {
        protected override void Configure(IDictionary<string, object> settings)
        {
            var assemblies = settings.GetAssemblies().ToList();

            var define = ConventionalRoutingConfiguration.New()
                .UseEndpointFilterer(new QueryAndCommandEndpointFilter())
                .UseRoutePolicy(new DefaultRoutePolicy())
                .Configure(assemblies, settings);

            var templateSource = new AggregatedTemplateSource(new EmbeddedTemplateSource(assemblies), new FileSystemTemplateSource(assemblies, new FileScanner()));

            var rendererHandler = OutputRendererBuilder.New()
                .When(x => x.GetRequest().Headers.Accept.Contains("text/html")).UseRenderer(RenderOutputUsingSpark.Configure(templateSource, settings))
                .When(x => true).UseRenderer(new RenderOutputAsJson())
                .Build();

            AddChain("chains.Partials", app =>
            {
                app.Use<MeasureInner>(new MeasureInnerOptions((time, environment) => Console.WriteLine("Executed partial in {0}ms.", (int)time.TotalMilliseconds)))
                    .Use<HandleExceptions>()
                    .Use<AuthorizeRequest>(new AuthorizeRequestOptions().WithAuthorizer(new TestAuthorizer()))
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>(rendererHandler);
            });

            AddChain("chains.Web", app =>
            {
                var container = settings.GetContainer();
                var diagnosticsManager = container.GetInstance<IManageDiagnosticsInformation>();

                app.Use<Diagnose>(new DiagnoseOptions(diagnosticsManager, "Urls", x => x.GetRequest().Uri.ToString()))
                    .Use<MeasureInner>(new MeasureInnerOptions((time, environment) => Console.WriteLine("Executed url: {0} in {1}ms.", environment.GetRequest().Uri.ToString(), (int)time.TotalMilliseconds)))
                    .Use<RedirectToCorrectUrl>(new RedirectToCorrectUrlOptions((url, environment) => url.ToLower()))
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
                        .AddCase(y => !(y.Get<ValidationResult>("superglue.ValidationResult") ?? new ValidationResult(new List<ValidationResult.ValidationError>())).IsValid, app
                            .New()
                            .Use<HandleValidationErrorMiddleware>()
                            .Build())
                        .AddCase(y => y.GetRouteInformation().RoutedTo == null, app
                            .New()
                            .Use<SetStatusCode>(404)
                            .Use<HandleNotFoundMiddleware>()
                            .Build()))
                    .Use<HandleExceptions>()
                    .Use<NestedStructureMapContainer>(container)
                    .Use<BindModels>(container.GetInstance<IModelBinderCollection>())
                    .Use<RouteUsingSuperscribe>(new RouteUsingSuperscribeOptions(define))
                    .Use<HandleUnitOfWork>()
                    .Use<AuthorizeRequest>(new AuthorizeRequestOptions().WithAuthorizer(new TestAuthorizer()))
                    .Use<ValidateRequest>(new ValidateRequestOptions().UsingValidator(new ValidateRequestInput()))
                    .Use<ExecuteEndpoint>()
                    .Use<RenderOutput>(rendererHandler);
            });
        }
    }
}