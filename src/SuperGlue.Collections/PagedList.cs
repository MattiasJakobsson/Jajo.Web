using System;
using System.Collections;
using System.Collections.Generic;

namespace SuperGlue.Collections
{
    public class PagedList<TItem> : IPagedList<TItem>
    {
        private readonly IEnumerable<TItem> _items;

        public PagedList(IEnumerable<TItem> items, long totalCount, int currentPage, int pageSize)
        {
            _items = items;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalCount = totalCount;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public long TotalCount { get; }
        public int CurrentPage { get; }
        public int PageSize { get; }
    }
}