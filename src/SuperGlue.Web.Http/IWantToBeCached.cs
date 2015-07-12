namespace SuperGlue.Web.Http
{
    public interface IWantToBeCached
    {
        CacheOptions GetCacheSettings();
    }
}