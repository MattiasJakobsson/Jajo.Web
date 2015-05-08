using System.IO;
using System.Net.Mime;
using Spark;

namespace OpenWeb.Output.Spark
{
    public class ContentType
    {
        public static readonly ContentType Html = new ContentType(MediaTypeNames.Text.Html);
        public static readonly ContentType Json = new ContentType("application/json");
        public static readonly ContentType Text = new ContentType(MediaTypeNames.Text.Plain);
        public static readonly ContentType Javascript = new ContentType("text/javascript");

        private readonly string _mimeType;

        public ContentType(string mimeType)
        {
            _mimeType = mimeType;
        }

        public override string ToString()
        {
            return _mimeType;
        }
    }
    public class ViewContext
    {
        public ViewContext(object model)
        {
            Model = model;
        }

        public object Model { get; private set; }
    }

    public abstract class OpenWebSparkView : SparkViewBase
    {
        protected OpenWebSparkView()
        {
            ContentType = ContentType.Html;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            SetModel(viewContext.Model);

            RenderView(writer);
        }

        internal IResolveDependencies ResolveDependencies { get; set; }
        public ContentType ContentType { get; set; }

        public T ContentRenderer<T>() where T : class
        {
            return ResolveDependencies.Resolve<T>();
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