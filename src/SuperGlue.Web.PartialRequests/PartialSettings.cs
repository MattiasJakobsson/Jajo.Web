using System;

namespace SuperGlue.Web.PartialRequests
{
    public class PartialSettings
    {
        private Func<object, bool> _isPartialCheck;

        public void CheckIfRequestIsPartialUsing(Func<object, bool> check)
        {
            _isPartialCheck = check;
        }

        internal bool IsPartialEndpoint(object endpoint)
        {
            return (_isPartialCheck ?? (x => false))(endpoint);
        }
    }
}