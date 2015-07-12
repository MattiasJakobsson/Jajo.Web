namespace SuperGlue.ApiDiscovery
{
    public class StateObject
    {
        public StateObject(string name, string value, string type)
        {
            Value = value;
            Type = type;
            Name = name;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Type { get; private set; }
    }
}