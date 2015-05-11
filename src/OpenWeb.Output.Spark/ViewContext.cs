using System.Collections.Generic;

namespace OpenWeb.Output.Spark
{
    public class ViewContext
    {
        public ViewContext(object model, IDictionary<string, object> environment, IDictionary<string, object> applicationSettings)
        {
            Model = model;
            Environment = environment;
            ApplicationSettings = applicationSettings;
        }

        public object Model { get; private set; }
        public IDictionary<string, object> Environment { get; private set; }
        public IDictionary<string, object> ApplicationSettings { get; private set; }
    }
}