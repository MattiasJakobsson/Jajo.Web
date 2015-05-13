using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Data;
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

        public void GetNextChunk(DateTime startSlice, Action<Tuple<TimeoutData, DateTime>> timeoutFound, out DateTime nextTimeToRunQuery)
        {
            var now = DateTime.UtcNow;

            using (var session = GetSession())
            {
                List<Tuple<TimeoutData, DateTime>> results;
                if (_lastCleanupTime == DateTime.MinValue || _lastCleanupTime.Add(_triggerCleanupEvery) < now)
                {
                    results = GetCleanupChunk(startSlice, session).ToList();
                }
                else
                {
                    results = new List<Tuple<TimeoutData, DateTime>>();
                }

                nextTimeToRunQuery = DateTime.UtcNow.AddMinutes(10);

                var query = GetChunkQuery(session)
                    .Where(t => t.Time > startSlice);

                QueryHeaderInformation qhi;
                using (var enumerator = session.Advanced.Stream(query, out qhi))
                {
                    while (enumerator.MoveNext())
                    {
                        var dateTime = enumerator.Current.Document.Time;
                        nextTimeToRunQuery = dateTime;

                        if (dateTime > DateTime.UtcNow) break;

                        results.Add(new Tuple<TimeoutData, DateTime>(enumerator.Current.Document.GetTimeoutData(), dateTime));
                    }
                }

                if (qhi != null && qhi.IsStale && results.Count == 0)
                    nextTimeToRunQuery = now;

                foreach (var result in results)
                {
                    timeoutFound(result);

                    session.Delete(result);
                }

                session.SaveChanges();
            }
        }

        private IRavenQueryable<RavenTimeOutData> GetChunkQuery(IDocumentSession session)
        {
            return session.Query<RavenTimeOutData, RavenTimeOutDataIndex>()
                .OrderBy(t => t.Time)
                .Where(
                    t =>
                        t.OwningTimeOutManager == String.Empty ||
                        t.OwningTimeOutManager == _timeoutManagerName);
        }

        private IEnumerable<Tuple<TimeoutData, DateTime>> GetCleanupChunk(DateTime startSlice, IDocumentSession session)
        {
            var chunk = GetChunkQuery(session)
                .Where(t => t.Time <= startSlice.Subtract(_cleanupGapFromTimeslice))
                .Take(1024)
                .ToList()
                .Select(arg => new Tuple<TimeoutData, DateTime>(arg.GetTimeoutData(), arg.Time));

            _lastCleanupTime = DateTime.UtcNow;

            return chunk;
        }

        private IDocumentSession GetSession()
        {
            return string.IsNullOrEmpty(_timeoutDataBase)
                ? _documentStore.OpenSession()
                : _documentStore.OpenSession(_timeoutDataBase);
        }

        public void Add(TimeoutData timeout)
        {
            var session = GetSession();

            using (session)
            {
                session.Store(new RavenTimeOutData
                {
                    Id = RavenTimeOutData.BuildId(timeout.Id),
                    CommitId = timeout.Id,
                    Message = timeout.Message,
                    MetaData = timeout.MetaData,
                    OwningTimeOutManager = _timeoutManagerName,
                    Time = timeout.Time,
                    WriteTo = timeout.WriteTo
                });

                session.SaveChanges();
            }
        }
    }
}