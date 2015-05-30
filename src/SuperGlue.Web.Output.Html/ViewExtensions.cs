using System;
using System.Linq.Expressions;
using HtmlTags;
using HtmlTags.Conventions.Elements;

namespace SuperGlue.Web.Output.Html
{
    public static class ViewExtensions
    {
        public static IElementGenerator<T> Tags<T>(this ISuperGlueView<T> view) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>();
        }

        public static HtmlTag InputFor<T>(this ISuperGlueView<T> view, Expression<Func<T, object>> expression) where T : class
        {
            return Tags(view).InputFor(expression, null, view.Model);
        }

        public static HtmlTag InputFor<T>(this ISuperGlueView view, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().InputFor(expression);
        }

        public static HtmlTag InputFor<T>(this ISuperGlueView view, T model, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().InputFor(expression, null, model);
        }

        public static HtmlTag LabelFor<T>(this ISuperGlueView<T> view, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().LabelFor(expression, null, view.Model);
        }

        public static HtmlTag LabelFor<T>(this ISuperGlueView view, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().LabelFor(expression);
        }

        public static HtmlTag LabelFor<T>(this ISuperGlueView view, T model, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().LabelFor(expression, null, model);
        }

        public static HtmlTag DisplayFor<T>(this ISuperGlueView<T> view, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().DisplayFor(expression, null, view.Model);
        }

        public static HtmlTag DisplayFor<T>(this ISuperGlueView view, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().DisplayFor(expression);
        }

        public static HtmlTag DisplayFor<T>(this ISuperGlueView view, T model, Expression<Func<T, object>> expression) where T : class
        {
            return view.Environment.Resolve<IElementGenerator<T>>().DisplayFor(expression, null, model);
        }
    }
}