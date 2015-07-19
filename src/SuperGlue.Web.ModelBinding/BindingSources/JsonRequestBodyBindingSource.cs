using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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

            var json = await streamReader.ReadToEndAsync();

            var outer = JObject.Parse(json);

            var result = new Dictionary<string, object>();

            SetEdgeValues(outer, "", result);

            _data = result;
        }

        private static void SetEdgeValues(JObject outer, string prefix, IDictionary<string, object> data)
        {
            foreach (var inner in outer.Properties())
            {
                switch (inner.Value.Type)
                {
                    case JTokenType.Object:
                        SetEdgeValues((JObject)inner.Value, string.Format("{0}{1}_", prefix, inner.Name), data);
                        break;
                    case JTokenType.Array:
                        HandleArray((JArray)inner.Value, string.Format("{0}{1}", prefix, inner.Name), data);
                        break;
                    default:
                        data[string.Format("{0}{1}", prefix, inner.Name)] = inner.Value.ToObject<object>();
                        break;
                }
            }
        }

        private static void HandleArray(JArray array, string prefix, IDictionary<string, object> data)
        {
            var index = 0;
            foreach (var child in array)
            {
                switch (child.Type)
                {
                    case JTokenType.Object:
                        SetEdgeValues((JObject)child, string.Format("{0}[{1}]_", prefix, index), data);
                        break;
                    case JTokenType.Array:
                        HandleArray((JArray)child, string.Format("{0}[{1}]", prefix, index), data);
                        break;
                    case JTokenType.Property:
                        data[string.Format("{0}[{1}]_{2}", prefix, index, ((JProperty)child).Name)] = child.ToObject<object>();
                        break;
                    default:
                        data[string.Format("{0}[{1}]_", prefix, index)] = child.ToObject<object>();
                        break;
                }

                index++;
            }
        }
    }
}