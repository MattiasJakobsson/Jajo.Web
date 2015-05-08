using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenWeb.Endpoints
{
    public interface IOpenWebContext
    {
        WebEnvironment Environment { get; }
        MethodInfo RoutedTo { get; }
        T Get<T>();
        void Set<T>(T data);
        IResolveDependencies DependencyResolver { get; set; }
    }

    public class OpenWebContext : IOpenWebContext
    {
        private readonly WebEnvironment _environment;

        public OpenWebContext(IDictionary<string, object> environment)
        {
            _environment = new WebEnvironment(environment);
        }

        public WebEnvironment Environment
        {
            get { return _environment; }
        }

        public MethodInfo RoutedTo
        {
            get { throw new NotImplementedException(); }
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
            throw new NotImplementedException();
        }

        public IResolveDependencies DependencyResolver
        {
            get { return Environment.Get<IResolveDependencies>("openweb.DependencyResolver"); }
            set { Environment.Set("openweb.DependencyResolver", value); }
        }
    }
}