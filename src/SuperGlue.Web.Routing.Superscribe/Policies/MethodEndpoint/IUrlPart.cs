namespace SuperGlue.Web.Routing.Superscribe.Policies.MethodEndpoint
{
    public interface IUrlPart
    {
        void AddToBuilder(IRouteBuilder builder);
    }
}