using System.Collections.Generic;

namespace SuperGlue.Web.Output.Spark
{
    public class ViewContext
    {
        public ViewContext(object model, IDictionary<string, object> environment)
        {
            Model = model;
            Environment = environment;
        }

        public object Model { get; private set; }
        public IDictionary<string, object> Environment { get; private set; }
    }
}