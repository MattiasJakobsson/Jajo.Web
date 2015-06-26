using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using SuperGlue.EventTracking;

namespace SuperGlue.RavenDb
{
    public class TrackingSession : IAsyncDocumentSession
    {
        private readonly IAsyncDocumentSession _innerSession;
        private readonly ITrackEntitiesWithEvents _trackEntitiesWithEvents;

        public TrackingSession(IAsyncDocumentSession innerSession, ITrackEntitiesWithEvents trackEntitiesWithEvents)
        {
            _trackEntitiesWithEvents = trackEntitiesWithEvents;
            _innerSession = innerSession;
        }

        public void Dispose()
        {
            _innerSession.Dispose();
        }

        public void Delete<T>(T entity)
        {
            _innerSession.Delete(entity);

            Loaded(entity);
        }

        public void Delete<T>(ValueType id)
        {
            _innerSession.Delete(id);
        }

        public void Delete(string id)
        {
            _innerSession.Delete(id);
        }

        public async Task<T> LoadAsync<T>(string id, CancellationToken token = default(CancellationToken))
        {
            var entity = await _innerSession.LoadAsync<T>(id, token);

            Loaded(entity);

            return entity;
        }

        public async Task<T[]> LoadAsync<T>(IEnumerable<string> ids, CancellationToken token = default(CancellationToken))
        {
            var entities = await _innerSession.LoadAsync<T>(ids, token);

            foreach (var entity in entities)
                Loaded(entity);

            return entities;
        }

        public async Task<T> LoadAsync<T>(ValueType id, CancellationToken token = default(CancellationToken))
        {
            var entity = await _innerSession.LoadAsync<T>(id, token);

            Loaded(entity);

            return entity;
        }

        public async Task<T[]> LoadAsync<T>(CancellationToken token = default(CancellationToken), params ValueType[] ids)
        {
            var entities = await _innerSession.LoadAsync<T>(token, ids);

            foreach (var entity in entities)
                Loaded(entity);

            return entities;
        }

        public async Task<T[]> LoadAsync<T>(IEnumerable<ValueType> ids, CancellationToken token = default(CancellationToken))
        {
            var entities = await _innerSession.LoadAsync<T>(ids, token);

            foreach (var entity in entities)
                Loaded(entity);

            return entities;
        }

        public IRavenQueryable<T> Query<T>(string indexName, bool isMapReduce = false)
        {
            return _innerSession.Query<T>(indexName, isMapReduce);
        }

        public IRavenQueryable<T> Query<T>()
        {
            return _innerSession.Query<T>();
        }

        public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
        {
            return _innerSession.Query<T, TIndexCreator>();
        }

        public IAsyncLoaderWithInclude<object> Include(string path)
        {
            return _innerSession.Include(path);
        }

        public IAsyncLoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return _innerSession.Include(path);
        }

        public IAsyncLoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, object>> path)
        {
            return _innerSession.Include<T, TInclude>(path);
        }

        public async Task<TResult> LoadAsync<TTransformer, TResult>(string id, CancellationToken token = default(CancellationToken)) where TTransformer : AbstractTransformerCreationTask, new()
        {
            var entity = await _innerSession.LoadAsync<TTransformer, TResult>(id, token: token);

            Loaded(entity);

            return entity;
        }

        public async Task<TResult> LoadAsync<TTransformer, TResult>(string id, Action<ILoadConfiguration> configure, CancellationToken token = default(CancellationToken)) where TTransformer : AbstractTransformerCreationTask, new()
        {
            var entity = await _innerSession.LoadAsync<TTransformer, TResult>(id, configure, token);

            Loaded(entity);

            return entity;
        }

        public async Task<TResult[]> LoadAsync<TTransformer, TResult>(IEnumerable<string> ids, Action<ILoadConfiguration> configure, CancellationToken token = default(CancellationToken)) where TTransformer : AbstractTransformerCreationTask, new()
        {
            var results = await _innerSession.LoadAsync<TTransformer, TResult>(ids, configure, token);

            foreach (var result in results)
                Loaded(result);

            return results;
        }

        public async Task<TResult> LoadAsync<TResult>(string id, string transformer, Action<ILoadConfiguration> configure, CancellationToken token = default(CancellationToken))
        {
            var result = await _innerSession.LoadAsync<TResult>(id, transformer, configure, token);

            Loaded(result);

            return result;
        }

        public async Task<TResult[]> LoadAsync<TResult>(IEnumerable<string> ids, string transformer, Action<ILoadConfiguration> configure = null, CancellationToken token = default(CancellationToken))
        {
            var results = await _innerSession.LoadAsync<TResult>(ids, transformer, configure, token);

            foreach (var result in results)
                Loaded(result);

            return results;
        }

        public async Task<TResult> LoadAsync<TResult>(string id, Type transformerType, Action<ILoadConfiguration> configure = null, CancellationToken token = default(CancellationToken))
        {
            var result = await _innerSession.LoadAsync<TResult>(id, transformerType, configure, token);

            Loaded(result);

            return result;
        }

        public async Task<TResult[]> LoadAsync<TResult>(IEnumerable<string> ids, Type transformerType, Action<ILoadConfiguration> configure = null, CancellationToken token = default(CancellationToken))
        {
            var results = await _innerSession.LoadAsync<TResult>(ids, transformerType, configure, token);

            foreach (var result in results)
                Loaded(result);

            return results;
        }

        public Task SaveChangesAsync(CancellationToken token = default(CancellationToken))
        {
            return _innerSession.SaveChangesAsync(token);
        }

        public async Task StoreAsync(object entity, Etag etag, CancellationToken token = default(CancellationToken))
        {
            await _innerSession.StoreAsync(entity, etag, token);

            Loaded(entity);
        }

        public async Task StoreAsync(object entity, Etag etag, string id, CancellationToken token = default(CancellationToken))
        {
            await _innerSession.StoreAsync(entity, etag, id, token);

            Loaded(entity);
        }

        public async Task StoreAsync(object entity, string id, CancellationToken token = new CancellationToken())
        {
            await _innerSession.StoreAsync(entity, id, token);

            Loaded(entity);
        }

        public async Task StoreAsync(dynamic entity, CancellationToken token = default(CancellationToken))
        {
            await _innerSession.StoreAsync(entity, token);

            Loaded(entity);
        }

        public IAsyncAdvancedSessionOperations Advanced { get { return _innerSession.Advanced; } }

        private void Loaded(object entity)
        {
            var canApplyEvents = entity as ICanApplyEvents;

            if (canApplyEvents != null)
                _trackEntitiesWithEvents.Track(canApplyEvents);
        }
    }
}