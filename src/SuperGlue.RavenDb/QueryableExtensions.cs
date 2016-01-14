using System;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task<IPagedList<TItem>> ToPagedListAsync<TItem>(this IQueryable<TItem> query, RavenQueryStatistics statistics, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var items = await query.Skip(skip).Take(pageSize).ToListAsync().ConfigureAwait(false);

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