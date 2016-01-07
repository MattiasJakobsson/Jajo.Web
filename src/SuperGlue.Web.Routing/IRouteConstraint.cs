namespace SuperGlue.Web.Routing
{
    public interface IRouteConstraint
    {
        bool IsValid(object routeTo);
    }
}