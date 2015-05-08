namespace OpenWeb.UnitOfWork
{
    public interface IOpenWebUnitOfWork
    {
        void Begin();
        void Commit();
    }
}