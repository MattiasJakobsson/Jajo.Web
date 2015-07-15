namespace SuperGlue.Diagnostics
{
    public class ObjectDiagnosticsValue : IDiagnosticsValue
    {
        private readonly object _value;

        public ObjectDiagnosticsValue(object value)
        {
            _value = value;
        }

        public string GetStringRepresentation()
        {
            return _value.ToString();
        }
    }
}