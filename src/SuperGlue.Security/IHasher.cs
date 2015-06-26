namespace SuperGlue.Security
{
    public interface IHasher
    {
        string Hash(string original, string algorithm);
        string Hash(string original, string algorithm, string salt);
        string GenerateRandomSalt();
    }
}