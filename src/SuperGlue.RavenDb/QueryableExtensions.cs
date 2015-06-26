using System;
using System.Linq;
using Raven.Client;
using SuperGlue.Collections;

namespace SuperGlue.RavenDb
{
    public static class QueryableExtensions
    {
        public static IPagedList<TItem> ToPagedList<TItem>(this IQueryable<TItem> query, RavenQueryStatistics statistics, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var items = query.Skip(skip).Take(pageSize).ToList();

            return new PagedList<TItem>(items, statistics.TotalResults, page, pageSize);
        }

        public static Lazy<IPagedList<TItem>> ToLazyPagedList<TItem>(this IQueryable<TItem> query, RavenQueryStatistics statistics, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var items = query.Skip(skip).Take(pageSize).Lazily();

            return new Lazy<IPagedList<TItem>>(() => new PagedList<TItem>(items.Value, statistics.TotalResults, page, pageSize));
        }
    }
}