using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SuperGlue.Web.ModelBinding.BindingSources
{
    public class JsonRequestBodyBindingSource : IBindingSource
    {
        private IDictionary<string, object> _data;

        public async Task<IDictionary<string, object>> GetValues(IDictionary<string, object> envinronment)
        {
            await SetValues(envinronment);

            return _data.ToDictionary(x => x.Key.ToLower(), x => x.Value);
        }

        private async Task SetValues(IDictionary<string, object> envinronment)
        {
            if (_data != null)
                return;

            var streamReader = new StreamReader(envinronment.GetRequest().Body, Encoding.UTF8, true, 4 * 1024, true);

            _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(await streamReader.ReadToEndAsync()) ?? new Dictionary<string, object>();
        }
    }
}