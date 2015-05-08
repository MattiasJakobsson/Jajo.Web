using System.Collections.Generic;
using System.IO;
using Spark;

namespace OpenWeb.Output.Spark
{
    public abstract class OpenWebSparkView : SparkViewBase
    {
        private IDictionary<string, object> _environment;

        protected OpenWebSparkView()
        {
            ContentType = ContentType.Html;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            SetModel(viewContext.Model);
            _environment = viewContext.Environment;

            RenderView(writer);
        }

        public ContentType ContentType { get; set; }

        public T ContentRenderer<T>() where T : class
        {
            return _environment.Resolve<T>();
        }

        protected virtual void SetModel(object model)
        {

        }
    }

    public abstract class OpenWebSparkView<TModel> : OpenWebSparkView where TModel : class
    {
        public TModel Model { get; private set; }

        protected override void SetModel(object model)
        {
            Model = model as TModel;
        }
    }

}