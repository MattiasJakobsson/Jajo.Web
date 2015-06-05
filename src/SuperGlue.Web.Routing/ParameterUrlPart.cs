namespace SuperGlue.Web.Routing
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