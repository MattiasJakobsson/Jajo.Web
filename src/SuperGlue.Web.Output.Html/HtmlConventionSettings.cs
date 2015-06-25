using System;
using System.Linq.Expressions;
using HtmlTags.Conventions;

namespace SuperGlue.Web.Output.Html
{
    public class HtmlConventionSettings
    {
        private readonly ProfileExpression _profileExpression;
        private readonly HtmlConventionLibrary _library;

        private static readonly Cache<Type, Func<HtmlConventionLibrary, object>> ElementGeneratorsCache = new Cache<Type, Func<HtmlConventionLibrary, object>>(x =>
            {
                var method = typeof(HtmlConventionLibrary).GetMethod("GeneratorFor").MakeGenericMethod(x);
                var libraryParameter = Expression.Parameter(typeof(HtmlConventionLibrary));

                return Expression.Lambda<Func<HtmlConventionLibrary, object>>(Expression.Call(libraryParameter, method), libraryParameter).Compile();
            });

        public HtmlConventionSettings()
        {
            _library = new HtmlConventionLibrary();
            _profileExpression = new ProfileExpression(_library, TagConstants.Default);
        }

        public ElementCategoryExpression Labels
        {
            get { return _profileExpression.Labels; }
        }

        public ElementCategoryExpression Displays
        {
            get { return _profileExpression.Displays; }
        }

        public ElementCategoryExpression Editors
        {
            get { return _profileExpression.Editors; }
        }

        internal HtmlConventionLibrary ConventionLibrary { get { return _library; } }

        internal object ElementGeneratorFor(Type model)
        {
            return ElementGeneratorsCache[model](_library);
        }
    }
}