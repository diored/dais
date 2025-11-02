using System.Security.Cryptography;

using DioRed.Common.Hash;

namespace DioRed.Dais.Core.Internal;

internal class SaltedPassword
{
    public const int DefaultSaltLength = 6;

    private static readonly HashGenerator _hashGenerator = HashGenerator.GetSha256();

    private SaltedPassword()
    {
    }

    public required string PasswordHash { get; init; }

    public required string Salt { get; init; }

    public static SaltedPassword Create(string password, byte[] salt)
    {
        byte[] passwordHash = _hashGenerator.GenerateHash(password, salt);

        return new SaltedPassword
        {
            PasswordHash = Convert.ToBase64String(passwordHash),
            Salt = Convert.ToBase64String(salt)
        };
    }

    public static SaltedPassword CreateWithRandomSalt(string password, int saltLength = DefaultSaltLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(saltLength, 1, nameof(saltLength));

        byte[] salt = RandomNumberGenerator.GetBytes(saltLength);

        return Create(password, salt);
    }
}