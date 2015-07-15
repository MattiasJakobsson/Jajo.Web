using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SuperGlue.Configuration;

namespace SuperGlue.MetaData
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class SetMetaData
    {
        private readonly AppFunc _next;

        public SetMetaData(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var metaData = new Dictionary<string, object>();
            var oldMetaData = environment.GetMetaData();

            foreach (var item in oldMetaData.MetaData)
                metaData[item.Key] = item.Value;

            var metaDataSuppliers = environment.ResolveAll<ISupplyRequestMetaData>();

            foreach (var supplier in metaDataSuppliers.Where(supplier => supplier.CanHandleChain(environment.GetCurrentChain().Name)))
            {
                var data = await supplier.GetMetaData(environment);

                foreach (var item in data)
                    metaData[item.Key] = item.Value;
            }

            environment[MetaDataEnvironmentExtensions.MetaDataConstants.RequestMetaData] = new RequestMetaData(new ReadOnlyDictionary<string, object>(metaData));

            await _next(environment);

            environment[MetaDataEnvironmentExtensions.MetaDataConstants.RequestMetaData] = oldMetaData;
        }
    }
}