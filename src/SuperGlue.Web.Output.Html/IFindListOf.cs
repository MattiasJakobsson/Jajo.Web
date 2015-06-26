using System;
using System.Collections.Generic;

namespace SuperGlue.Web.Output.Html
{
    public interface IFindListOf<T>
    {
        IEnumerable<SelectListItem> Find();
    }

    public interface IFindListOf
    {
        IEnumerable<SelectListItem> Find(Type type);
    }
}