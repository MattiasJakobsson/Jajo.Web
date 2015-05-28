namespace SuperGlue.Web.Routing.Superscribe.Policies.MethodEndpoint
{
    public class StaticUrlPart : IUrlPart
    {
        private readonly string _part;

        public StaticUrlPart(string part)
        {
            _part = part;
        }

        public void AddToBuilder(IRouteBuilder builder)
        {
            builder.Append(_part);
        }
    }
}