namespace SuperGlue.Web.StaticFiles
{
    public interface IReadFiles
    {
        ReadResult TryRead(string path);
    }
}