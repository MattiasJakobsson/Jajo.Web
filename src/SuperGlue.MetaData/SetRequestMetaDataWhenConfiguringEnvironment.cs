using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEndThings> Begin(IDictionary<string, object> environment, Type middlewareType)
        {
            var metaData = new Dictionary<string, object>();
            var oldMetaData = environment.GetMetaData();

            foreach (var item in oldMetaData.MetaData)
                metaData[item.Key] = item.Value;

            foreach (var supplier in _metaDataSuppliers.Where(supplier => supplier.CanHandleChain(environment.GetCurrentChain().Name)))
            {
                var data = await supplier.GetMetaData(environment);

                foreach (var item in data)
                    metaData[item.Key] = item.Value;
            }

            environment[MetaDataEnvironmentExtensions.MetaDataConstants.RequestMetaData] = new RequestMetaData(new ReadOnlyDictionary<string, object>(metaData));

            return new Disposable(environment, oldMetaData);
        }

        private class Disposable : IEndThings
        {
            private readonly IDictionary<string, object> _environment;
            private readonly RequestMetaData _oldMetaData;

            public Disposable(IDictionary<string, object> environment, RequestMetaData oldMetaData)
            {
                _environment = environment;
                _oldMetaData = oldMetaData;
            }

            public Task End()
            {
                _environment[MetaDataEnvironmentExtensions.MetaDataConstants.RequestMetaData] = _oldMetaData;

                return Task.CompletedTask;
            }
        }
    }
}