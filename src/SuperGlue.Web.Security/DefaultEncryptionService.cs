using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SuperGlue.Web.Security
{
    public class DefaultEncryptionService : IEncryptionService
    {
        private const string Key = "AsgASH3EDasH5SA6asfASG7aH687daH";

        public string Encrypt(string value, string salt)
        {
            string outStr;
            RijndaelManaged aesAlg = null;

            try
            {
                aesAlg = GetManager(salt);

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(value);

                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return outStr;

        }

        public string Decrypt(string encrypted, string salt)
        {
            RijndaelManaged aesAlg = null;
            string plaintext;

            try
            {
                aesAlg = GetManager(salt);
                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                var bytes = Convert.FromBase64String(encrypted);
                using (var msDecrypt = new MemoryStream(bytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                        plaintext = srDecrypt.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return "";
            }
            finally
            {
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        private static RijndaelManaged GetManager(string salt)
        {
            var deriveBytes = new Rfc2898DeriveBytes(Key, Encoding.ASCII.GetBytes(salt));

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = deriveBytes.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = deriveBytes.GetBytes(aesAlg.BlockSize / 8);

            return aesAlg;
        }
    }
}