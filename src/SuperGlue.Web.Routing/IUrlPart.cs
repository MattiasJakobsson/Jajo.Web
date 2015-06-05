namespace SuperGlue.Web.Routing
{
    public interface IUrlPart
    {
        void AddToBuilder(IRouteBuilder builder);
    }
}