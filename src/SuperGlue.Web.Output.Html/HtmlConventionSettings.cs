using System;
using System.Linq.Expressions;
using HtmlTags.Conventions;

namespace SuperGlue.Web.Output.Html
{
    public class HtmlConventionSettings
    {
        private readonly ProfileExpression _profileExpression;

        private static readonly Cache<Type, Func<HtmlConventionLibrary, object>> ElementGeneratorsCache = new Cache<Type, Func<HtmlConventionLibrary, object>>(x =>
            {
                var method = typeof(HtmlConventionLibrary).GetMethod("GeneratorFor").MakeGenericMethod(x);
                var libraryParameter = Expression.Parameter(typeof(HtmlConventionLibrary));

                return Expression.Lambda<Func<HtmlConventionLibrary, object>>(Expression.Call(libraryParameter, method), libraryParameter).Compile();
            });

        public HtmlConventionSettings()
        {
            ConventionLibrary = new HtmlConventionLibrary();
            _profileExpression = new ProfileExpression(ConventionLibrary, TagConstants.Default);
        }

        public ElementCategoryExpression Labels => _profileExpression.Labels;

        public ElementCategoryExpression Displays => _profileExpression.Displays;

        public ElementCategoryExpression Editors => _profileExpression.Editors;

        public ElementCategoryExpression Forms => new ElementCategoryExpression(ConventionLibrary.TagLibrary.Category("Form").Profile(TagConstants.Default));

        internal HtmlConventionLibrary ConventionLibrary { get; }

        internal object ElementGeneratorFor(Type model)
        {
            return ElementGeneratorsCache[model](ConventionLibrary);
        }
    }
}