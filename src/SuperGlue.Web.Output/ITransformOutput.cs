using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
	public interface ITransformOutput
	{
		Task<OutputRenderingResult> Transform(OutputRenderingResult result, IDictionary<string, object> environment);
	}
}