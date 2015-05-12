using System.Collections.Generic;
using System.IO;
using Spark;

namespace Jajo.Web.Output.Spark
{
    public abstract class JajoSparkView : SparkViewBase
    {
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            SetModel(viewContext.Model);
            Environment = viewContext.Environment;

            RenderView(writer);
        }

        public IDictionary<string, object> Environment { get; private set; }

        public string RouteTo(object input)
        {
            return Environment.RouteTo(input);
        }

        protected virtual void SetModel(object model)
        {

        }
    }

    public abstract class JajoSparkView<TModel> : JajoSparkView where TModel : class
    {
        public TModel Model { get; private set; }

        protected override void SetModel(object model)
        {
            Model = model as TModel;
        }
    }

}