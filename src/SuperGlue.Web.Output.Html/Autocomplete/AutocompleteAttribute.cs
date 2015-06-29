using System;

namespace SuperGlue.Web.Output.Html.Autocomplete
{
    public class AutocompleteAttribute : Attribute
    {
        public AutocompleteAttribute(string remote)
        {
            Remote = remote;
        }

        public string Remote { get; private set; }
    }
}