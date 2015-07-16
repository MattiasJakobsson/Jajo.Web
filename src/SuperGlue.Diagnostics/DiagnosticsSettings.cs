using System.Collections.Generic;

namespace SuperGlue.Diagnostics
{
    public class DiagnosticsSettings
    {
        private readonly ICollection<string> _allowedKeys = new List<string>();
        private readonly ICollection<string> _disAllowedKeys = new List<string>();
        private bool _allowAll;
        private bool _disAllowAll;

        public DiagnosticsSettings Allow(string key)
        {
            _allowedKeys.Add((key ?? "").ToLower());

            return this;
        }

        public DiagnosticsSettings Disallow(string key)
        {
            _disAllowedKeys.Add((key ?? "").ToLower());

            return this;
        }

        public DiagnosticsSettings AllowAll()
        {
            _allowAll = true;

            return this;
        }

        public DiagnosticsSettings DisllowAll()
        {
            _disAllowAll = true;

            return this;
        }

        public DiagnosticsSettings ResetGeneral()
        {
            _allowAll = false;
            _disAllowAll = false;

            return this;
        }

        internal bool IsKeyAllowed(string key)
        {
            if (_disAllowAll)
                return false;

            if (_allowAll)
                return true;

            return !_disAllowedKeys.Contains((key ?? "").ToLower()) && _allowedKeys.Contains((key ?? "").ToLower());
        }
    }
}