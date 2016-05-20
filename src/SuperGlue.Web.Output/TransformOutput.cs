using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperGlue.Web.Output
{
	using AppFunc = Func<IDictionary<string, object>, Task>;

	public class TransformOutput
	{
		private readonly AppFunc _next;

		public TransformOutput(AppFunc next)
		{
			if (next == null)
				throw new ArgumentNullException(nameof(next));

			_next = next;
		}

		public async Task Invoke(IDictionary<string, object> environment)
		{
			var transformers = environment.ResolveAll<ITransformOutput>();

			var output = environment.GetOutputResult();

			if (output != null)
			{
				foreach (var transformer in transformers)
					output = await transformer.Transform(output, environment);

				environment.SetOutputResult(output);
			}

			await _next(environment).ConfigureAwait(false);
		}
	}
}