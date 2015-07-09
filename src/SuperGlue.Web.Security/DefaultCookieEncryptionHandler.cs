using System;
using System.Collections.Generic;
using System.Web;

namespace SuperGlue.Web.Security
{
    public class DefaultCookieEncryptionHandler : IHandleEncryptedCookies
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IDictionary<string, object> _environment;

        public DefaultCookieEncryptionHandler(IEncryptionService encryptionService, IDictionary<string, object> environment)
        {
            _encryptionService = encryptionService;
            _environment = environment;
        }

        public void Write(string name, string information, DateTime? expires = null, bool secure = false, bool httpOnly = false, string path = "/", string domain = null)
        {
            if (string.IsNullOrEmpty(name))
                return;

            var salt = _environment.GetRequest().Uri.DnsSafeHost;

            _environment.GetResponse().Cookies.Append(name, HttpUtility.UrlEncode(_encryptionService.Encrypt(information, salt)),
                new WebEnvironmentExtensions.CookieOptions
                {
                    Expires = expires,
                    Secure = secure,
                    Path = path,
                    HttpOnly = httpOnly,
                    Domain = domain
                });
        }

        public string Read(string name)
        {
            var cookie = _environment.GetRequest().Cookies[name];

            if (string.IsNullOrEmpty(cookie)) 
                return null;

            var salt = _environment.GetRequest().Uri.DnsSafeHost;

            return _encryptionService.Decrypt(HttpUtility.UrlDecode(cookie), salt);
        }

        public void Remove(string name)
        {
            _environment.GetResponse().Cookies.Append(name, "", new WebEnvironmentExtensions.CookieOptions
            {
                Expires = DateTime.MinValue
            });
        }
    }
}