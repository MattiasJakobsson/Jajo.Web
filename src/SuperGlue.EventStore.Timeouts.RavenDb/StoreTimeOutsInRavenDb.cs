using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Abstractions.Data;
using Raven.Abstractions.Extensions;
using Raven.Client;
using Raven.Client.Linq;

namespace SuperGlue.EventStore.Timeouts.RavenDb
{
    public class StoreTimeOutsInRavenDb : IStoreTimeouts
    {
        private readonly IDocumentStore _documentStore;
        private readonly string _timeoutDataBase;
        private readonly string _timeoutManagerName;
        private DateTime _lastCleanupTime = DateTime.MinValue;
        private readonly TimeSpan _cleanupGapFromTimeslice;
        private readonly TimeSpan _triggerCleanupEvery;

        public StoreTimeOutsInRavenDb(IDocumentStore documentStore, string timeoutManagerName, string timeoutDataBase)
        {
            _documentStore = documentStore;
            _timeoutManagerName = timeoutManagerName;
            _timeoutDataBase = timeoutDataBase;
            _triggerCleanupEvery = TimeSpan.FromMinutes(2);
            _cleanupGapFromTimeslice = TimeSpan.FromMinutes(1);
        }

        public async Task<DateTime> GetNextChunk(DateTime startSlice, Func<Tuple<TimeoutData, DateTime>, Task> timeoutFound)
        {
            var now = DateTime.UtcNow;

            using (var session = GetSession())
            {
                List<Tuple<TimeoutData, DateTime>> results;
                if (_lastCleanupTime == DateTime.MinValue || _lastCleanupTime.Add(_triggerCleanupEvery) < now)
                {
                    results = (await GetCleanupChunk(startSlice, session).ConfigureAwait(false)).ToList();
                }
                else
                {
                    results = new List<Tuple<TimeoutData, DateTime>>();
                }

               var nextTimeToRunQuery = DateTime.UtcNow.AddMinutes(10);

                var query = GetChunkQuery(session)
                    .Where(t => t.Time > startSlice);

                var qhi = new Reference<QueryHeaderInformation>();
                using (var enumerator = await session.Advanced.StreamAsync(query, qhi).ConfigureAwait(false))
                {
                    while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        var dateTime = enumerator.Current.Document.Time;
                        nextTimeToRunQuery = dateTime;

                        if (dateTime > DateTime.UtcNow) break;

                        results.Add(new Tuple<TimeoutData, DateTime>(enumerator.Current.Document.GetTimeoutData(), dateTime));
                    }
                }

                if (qhi.Value != null && qhi.Value.IsStale && results.Count == 0)
                    nextTimeToRunQuery = now;

                foreach (var result in results)
                {
                    await timeoutFound(result).ConfigureAwait(false);

                    session.Delete(result);
                }

                await session.SaveChangesAsync().ConfigureAwait(false);

                return nextTimeToRunQuery;
            }
        }

        private IQueryable<RavenTimeOutData> GetChunkQuery(IAsyncDocumentSession session)
        {
            return session.Query<RavenTimeOutData, RavenTimeOutDataIndex>()
                .OrderBy(t => t.Time)
                .Where(
                    t =>
                        t.OwningTimeOutManager == string.Empty ||
                        t.OwningTimeOutManager == _timeoutManagerName);
        }

        private async Task<IEnumerable<Tuple<TimeoutData, DateTime>>> GetCleanupChunk(DateTime startSlice, IAsyncDocumentSession session)
        {
            var chunk = (await GetChunkQuery(session)
                .Where(t => t.Time <= startSlice.Subtract(_cleanupGapFromTimeslice))
                .Take(1024)
                .ToListAsync().ConfigureAwait(false))
                .Select(arg => new Tuple<TimeoutData, DateTime>(arg.GetTimeoutData(), arg.Time));

            _lastCleanupTime = DateTime.UtcNow;

            return chunk;
        }

        private IAsyncDocumentSession GetSession()
        {
            return string.IsNullOrEmpty(_timeoutDataBase)
                ? _documentStore.OpenAsyncSession()
                : _documentStore.OpenAsyncSession(_timeoutDataBase);
        }

        public async Task Add(TimeoutData timeout)
        {
            var session = GetSession();

            using (session)
            {
                await session.StoreAsync(new RavenTimeOutData
                {
                    Id = RavenTimeOutData.BuildId(timeout.Id),
                    CommitId = timeout.Id,
                    Message = timeout.Message,
                    MetaData = timeout.MetaData,
                    OwningTimeOutManager = _timeoutManagerName,
                    Time = timeout.Time,
                    WriteTo = timeout.WriteTo
                }).ConfigureAwait(false);

                await session.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}