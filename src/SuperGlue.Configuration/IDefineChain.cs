namespace SuperGlue.Configuration
{
    public interface IDefineChain
    {
        string Name { get; }
        void Define(IBuildAppFunction app);
        void AlterSettings(ChainSettings settings);
    }
}