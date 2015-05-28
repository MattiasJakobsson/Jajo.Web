namespace SuperGlue.Web.Routing.Superscribe.Policies.MethodEndpoint
{
    public class ParameterUrlPart : IUrlPart
    {
        private readonly RouteParameter _parameter;

        public ParameterUrlPart(RouteParameter parameter)
        {
            _parameter = parameter;
        }

        public void AddToBuilder(IRouteBuilder builder)
        {
            builder.AppendParameter(_parameter);
        }
    }
}