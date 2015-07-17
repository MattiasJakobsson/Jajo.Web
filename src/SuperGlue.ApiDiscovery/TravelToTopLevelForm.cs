namespace SuperGlue.ApiDiscovery
{
    public class TravelToTopLevelForm : IFormTravelInstruction
    {
        private readonly string _name;

        public TravelToTopLevelForm(string name)
        {
            _name = name;
        }

        public ApiForm TravelTo(ApiResource resource)
        {
            return !resource.Forms.ContainsKey(_name) ? null : resource.Forms[_name];
        }
    }
}