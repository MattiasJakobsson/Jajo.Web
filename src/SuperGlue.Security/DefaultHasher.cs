using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SuperGlue.Security
{
    public class DefaultHasher : IHasher
    {
        private static readonly IDictionary<string, Func<string, string>> HashAlgorithms = new Dictionary<string, Func<string, string>>
        {
            {"SHA1", HashUsingSha1},
            {"BCRYPT", HashUsingBcrypt}
        };

        private readonly HasherSettings _settings;

        public DefaultHasher(HasherSettings settings)
        {
            _settings = settings;
        }

        public string Hash(string original, string algorithm)
        {
            return Hash(original, algorithm, string.Empty);
        }

        public string Hash(string original, string algorithm, string salt)
        {
            var algorithmToUse = (algorithm ?? "").ToUpper();
            var input = string.Format("{0}{1}{2}", original, _settings.Key, salt);

            return HashAlgorithms.ContainsKey(algorithmToUse) ? HashAlgorithms[algorithmToUse](input) : HashAlgorithms.First().Value(input);
        }

        public string GenerateRandomSalt()
        {
            return Guid.NewGuid().ToString();
        }

        private static string HashUsingSha1(string input)
        {
            var hasher = new SHA1Managed();
            var passwordBytes = Encoding.ASCII.GetBytes(input);
            var passwordHash = hasher.ComputeHash(passwordBytes);
            return Convert.ToBase64String(passwordHash);
        }

        private static string HashUsingBcrypt(string input)
        {
            return BCrypt.Net.BCrypt.HashString(input, 11);
        }
    }
}