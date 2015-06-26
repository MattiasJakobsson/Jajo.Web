using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Output.Html
{
    public class DefaultListFinder : IFindListOf
    {
        private readonly IDictionary<string, object> _environment;

        public DefaultListFinder(IDictionary<string, object> environment)
        {
            _environment = environment;
        }

        public IEnumerable<SelectListItem> Find(Type type)
        {
            var finder = _environment.Resolve(typeof(IFindListOf<>).MakeGenericType(type));

            if (finder == null)
                return new List<SelectListItem>();

            //TODO:Refactor
            return (IEnumerable<SelectListItem>)finder.GetType().GetMethod("Find", new Type[0]).Invoke(finder, new object[0]);
        }
    }
}