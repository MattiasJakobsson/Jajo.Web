using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SuperGlue.ExceptionManagement
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Retry
    {
        private readonly AppFunc _next;
        private readonly int _retryTimes;
        private readonly TimeSpan _retryInterval;
        private readonly string _description;

        public Retry(AppFunc next, int retryTimes, TimeSpan retryInterval, string description)
        {
            if (next == null)
                throw new ArgumentNullException("next");

            if (retryTimes < 1)
                throw new ArgumentException("You have to try atleast once", "retryTimes");

            _next = next;
            _retryTimes = retryTimes;
            _retryInterval = retryInterval;
            _description = description;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var tries = 0;
            Exception lastException = null;

            while (tries < _retryTimes)
            {
                try
                {
                    await _next(environment);

                    lastException = null;

                    break;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                await Task.Delay(_retryInterval);

                tries++;
            }

            if (lastException != null)
                throw new RetryException(_retryTimes, _description, lastException);
        }
    }
}