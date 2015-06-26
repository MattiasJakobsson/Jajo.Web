using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using SuperGlue.EventTracking;

namespace SuperGlue.RavenDb
{
    public class TrackingSyncSession : IDocumentSession
    {
        private readonly IDocumentSession _innerSession;
        private readonly ITrackEntitiesWithEvents _trackEntitiesWithEvents;
        private readonly object _currentCommand;
        private readonly IDictionary<string, object> _currentMetaData;

        public TrackingSyncSession(IDocumentSession innerSession, ITrackEntitiesWithEvents trackEntitiesWithEvents, object currentCommand, IDictionary<string, object> currentMetaData)
        {
            _trackEntitiesWithEvents = trackEntitiesWithEvents;
            _currentCommand = currentCommand;
            _currentMetaData = currentMetaData;
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

        public T Load<T>(string id)
        {
            var entity = _innerSession.Load<T>(id);

            Loaded(entity);

            return entity;
        }

        public T[] Load<T>(params string[] ids)
        {
            var entities = _innerSession.Load<T>(ids);

            foreach (var entity in entities)
                Loaded(entity);

            return entities;
        }

        public T[] Load<T>(IEnumerable<string> ids)
        {
            var entities = _innerSession.Load<T>(ids);

            foreach (var entity in entities)
                Loaded(entity);

            return entities;
        }

        public T Load<T>(ValueType id)
        {
            var entity = _innerSession.Load<T>(id);

            Loaded(entity);

            return entity;
        }

        public T[] Load<T>(params ValueType[] ids)
        {
            var entities = _innerSession.Load<T>(ids);

            foreach (var entity in entities)
                Loaded(entity);

            return entities;
        }

        public T[] Load<T>(IEnumerable<ValueType> ids)
        {
            var entities = _innerSession.Load<T>(ids);

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

        public ILoaderWithInclude<object> Include(string path)
        {
            return _innerSession.Include(path);
        }

        public ILoaderWithInclude<T> Include<T>(Expression<Func<T, object>> path)
        {
            return _innerSession.Include(path);
        }

        public ILoaderWithInclude<T> Include<T, TInclude>(Expression<Func<T, object>> path)
        {
            return _innerSession.Include<T, TInclude>(path);
        }

        public TResult Load<TTransformer, TResult>(string id) where TTransformer : AbstractTransformerCreationTask, new()
        {
            var entity = _innerSession.Load<TTransformer, TResult>(id);

            Loaded(entity);

            return entity;
        }

        public TResult Load<TTransformer, TResult>(string id, Action<ILoadConfiguration> configure) where TTransformer : AbstractTransformerCreationTask, new()
        {
            var entity = _innerSession.Load<TTransformer, TResult>(id, configure);

            Loaded(entity);

            return entity;
        }

        public TResult[] Load<TTransformer, TResult>(params string[] ids) where TTransformer : AbstractTransformerCreationTask, new()
        {
            var results = _innerSession.Load<TTransformer, TResult>(ids);

            foreach (var result in results)
                Loaded(result);

            return results;
        }

        public TResult[] Load<TTransformer, TResult>(IEnumerable<string> ids, Action<ILoadConfiguration> configure) where TTransformer : AbstractTransformerCreationTask, new()
        {
            var results = _innerSession.Load<TTransformer, TResult>(ids, configure);

            foreach (var result in results)
                Loaded(result);

            return results;
        }

        public TResult Load<TResult>(string id, string transformer, Action<ILoadConfiguration> configure)
        {
            var result = _innerSession.Load<TResult>(id, transformer, configure);

            Loaded(result);

            return result;
        }

        public TResult[] Load<TResult>(IEnumerable<string> ids, string transformer, Action<ILoadConfiguration> configure = null)
        {
            var results = _innerSession.Load<TResult>(ids, transformer, configure);

            foreach (var result in results)
                Loaded(result);

            return results;
        }

        public TResult Load<TResult>(string id, Type transformerType, Action<ILoadConfiguration> configure = null)
        {
            var result = _innerSession.Load<TResult>(id, transformerType, configure);

            Loaded(result);

            return result;
        }

        public TResult[] Load<TResult>(IEnumerable<string> ids, Type transformerType, Action<ILoadConfiguration> configure = null)
        {
            var results = _innerSession.Load<TResult>(ids, transformerType, configure);

            foreach (var result in results)
                Loaded(result);

            return results;
        }

        public void SaveChanges()
        {
            _innerSession.SaveChanges();
        }

        public void Store(object entity, Etag etag)
        {
            _innerSession.Store(entity, etag);

            Loaded(entity);
        }

        public void Store(object entity, Etag etag, string id)
        {
            _innerSession.Store(entity, etag, id);

            Loaded(entity);
        }

        public void Store(dynamic entity)
        {
            _innerSession.Store(entity);

            Loaded(entity);
        }

        public void Store(dynamic entity, string id)
        {
            _innerSession.Store(entity, id);

            Loaded(entity);
        }

        public ISyncAdvancedSessionOperation Advanced { get { return _innerSession.Advanced; } }

        private void Loaded(object entity)
        {
            var canApplyEvents = entity as ICanApplyEvents;

            if (canApplyEvents != null)
                _trackEntitiesWithEvents.Track(canApplyEvents, _currentCommand, _currentMetaData);
        }
    }
}