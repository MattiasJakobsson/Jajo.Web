namespace SuperGlue.ApiDiscovery
{
    public class ApiDiscoverySettings
    {
        public ApiDefinition RegistrationRoot { get; private set; }

        public ApiDiscoverySettings RegisterAt(ApiDefinition apiDefinition)
        {
            RegistrationRoot = apiDefinition;
            return this;
        }
    }
}