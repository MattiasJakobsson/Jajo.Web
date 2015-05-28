using Superscribe.Models;

namespace SuperGlue.Web.Routing.Superscribe
{
    public class Guid : ParamNode<System.Guid>
    {
        public Guid(string name) : base(name)
        {
        }

        public static explicit operator Guid(string name)
        {
            return new Guid(name);
        }
    }
}