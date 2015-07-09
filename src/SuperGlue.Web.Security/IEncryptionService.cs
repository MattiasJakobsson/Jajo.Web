namespace SuperGlue.Web.Security
{
    public interface IEncryptionService
    {
        string Encrypt(string value, string salt);
        string Decrypt(string encrypted, string salt);
    }
}