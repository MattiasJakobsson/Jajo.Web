namespace SuperGlue.RavenDb.Search
{
    public class SearchPart
    {
        public SearchPart(string name, string value)
        {
            Value = value;
            Name = name;
        }

        public string Name { get; }
        public string Value { get; }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var searchPart = obj as SearchPart;

            if (searchPart == null)
                return false;

            return searchPart.Name == Name && searchPart.Value == Value;
        }
    }
}