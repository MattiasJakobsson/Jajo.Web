namespace SuperGlue.Web.Output.Html
{
    public class SelectListItem
    {
        public SelectListItem(string key, string text)
        {
            Text = text;
            Key = key;
        }

        public string Key { get; private set; }
        public string Text { get; private set; }
    }
}