using System.Collections;
using System.Collections.Generic;

namespace SuperGlue.Collections
{
    public interface IPagedList<out TItem> : IPagedList, IEnumerable<TItem>
    {

    }

    public interface IPagedList : IEnumerable
    {
        int TotalPages { get; }
        long TotalCount { get; }
        int CurrentPage { get; }
        int PageSize { get; }
    }
}