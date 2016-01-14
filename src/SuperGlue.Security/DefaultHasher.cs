using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SuperGlue.Security
{
    public class DefaultHasher : IHasher
    {
        private static readonly IDictionary<string, Func<string, string>> AvailableHashAlgorithms = new Dictionary<string, Func<string, string>>
        {
            {HashAlgorithms.Sha1, HashUsingSha1},
            {HashAlgorithms.Bcrypt, HashUsingBcrypt}
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
            var input = $"{original}{_settings.Key}{salt}";

            return AvailableHashAlgorithms.ContainsKey(algorithmToUse) ? AvailableHashAlgorithms[algorithmToUse](input) : AvailableHashAlgorithms.First().Value(input);
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