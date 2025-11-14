using Isopoh.Cryptography.Argon2;

namespace Dais.Core.Security;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return Argon2.Hash(password);
    }

    public bool Verify(string password, string encoded)
    {
        return Argon2.Verify(encoded, password);
    }
}