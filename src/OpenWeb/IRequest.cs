using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace OpenWeb
{
    public interface IRequest
    {
        IReadOnlyDictionary<string, object> Parameters { get; }
        IReadOnlyDictionary<string, object> RequestParameters { get; }
        MethodInfo RoutedTo { get; }
        T Get<T>();
        T Resolve<T>();
        object Resolve(Type service);
        void Set<T>(T data);
    }

    public class Request : IRequest
    {
        private readonly IDictionary<string, object> _environment;
        private readonly IDictionary<string, object> _requestParameters;

        public Request(IDictionary<string, object> environment)
        {
            _environment = environment;
            _environment["openweb.RequestParameters"] = _requestParameters = new ConcurrentDictionary<string, object>();
        }

        public IReadOnlyDictionary<string, object> Parameters
        {
            get { return new ReadOnlyDictionary<string, object>(Get<IDictionary<string, object>>("route.Parameters")); }
        }

        public IReadOnlyDictionary<string, object> RequestParameters
        {
            get { return new ReadOnlyDictionary<string, object>(_requestParameters); }
        }

        public MethodInfo RoutedTo
        {
            get { return Get<MethodInfo>("route.Method"); }
        }

        public T Get<T>()
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>()
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type service)
        {
            throw new NotImplementedException();
        }

        public void Set<T>(T data)
        {
            _requestParameters[typeof (T).FullName] = data;
        }

        private T Get<T>(string key)
        {
            object obj;
            if (!_environment.TryGetValue(key, out obj))
                return default(T);

            return (T)obj;
        }
    }
}