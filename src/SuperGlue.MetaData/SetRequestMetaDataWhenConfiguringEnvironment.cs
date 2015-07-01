using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SuperGlue.Configuration;

namespace SuperGlue.MetaData
{
    public class SetRequestMetaDataWhenConfiguringEnvironment : IWrapMiddleware<ConfigureEnvironment>
    {
        private readonly IEnumerable<ISupplyRequestMetaData> _metaDataSuppliers;

        public SetRequestMetaDataWhenConfiguringEnvironment(IEnumerable<ISupplyRequestMetaData> metaDataSuppliers)
        {
            _metaDataSuppliers = metaDataSuppliers;
        }

        public IDisposable Begin(IDictionary<string, object> environment)
        {
            var metaData = new Dictionary<string, object>();

            foreach (var item in _metaDataSuppliers.SelectMany(supplier => supplier.GetMetaData(environment)))
                metaData[item.Key] = item.Value;

            var oldMetaData = environment.GetMetaData();
            environment[MetaDataEnvironmentExtensions.MetaDataConstants.RequestMetaData] = new RequestMetaData(new ReadOnlyDictionary<string, object>(metaData));

            return new Disposable(environment, oldMetaData);
        }

        private class Disposable : IDisposable
        {
            private readonly IDictionary<string, object> _environment;
            private readonly RequestMetaData _oldMetaData;

            public Disposable(IDictionary<string, object> environment, RequestMetaData oldMetaData)
            {
                _environment = environment;
                _oldMetaData = oldMetaData;
            }

            public void Dispose()
            {
                _environment[MetaDataEnvironmentExtensions.MetaDataConstants.RequestMetaData] = _oldMetaData;
            }
        }
    }
}