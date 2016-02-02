namespace SuperGlue.ContentParsing
{
    public interface IFindParameterValueFromModel
    {
        object Find(string parameter, object model);
    }
}