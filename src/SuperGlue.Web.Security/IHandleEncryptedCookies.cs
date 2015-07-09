using System;

namespace SuperGlue.Web.Security
{
    public interface IHandleEncryptedCookies
    {
        void Write(string name, string information, DateTime? expires = null, bool secure = false, bool httpOnly = false, string path = "/", string domain = null);
        string Read(string name);
        void Remove(string name);
    }
}