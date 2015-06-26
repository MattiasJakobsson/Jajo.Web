using System;

namespace SuperGlue.Web.Output.Html
{
    public class ValueOfAttribute : Attribute
    {
        public ValueOfAttribute(Type itemType, string filteredBy = null)
        {
            FilteredBy = filteredBy;
            ItemType = itemType;
        }

        public Type ItemType { get; private set; }
        public string FilteredBy { get; private set; }
    }
}