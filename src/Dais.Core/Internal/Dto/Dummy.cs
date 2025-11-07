using System.Security.Cryptography;

using Isopoh.Cryptography.Argon2;

namespace DioRed.Dais.Core.Internal.Dto;

/// <summary>
/// Helps to prevent timing attacks
/// </summary>
internal static class Dummy
{
    static Dummy()
    {
        string id = IdGenerator.Generate();
        string randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(36));

        User = new UserDto
        {
            Id = id,
            UserName = "dummy_user",
            DisplayName = "Dummy User",
            PasswordHash = Argon2.Hash(randomPassword)
        };

        Client = new ClientDto
        {
            Id = id,
            OwnerId = "",
            ClientId = "dummy_client",
            ClientSecretHash = Argon2.Hash(randomPassword),
            DisplayName = "Dummy Client",
            Callbacks = []
        };
    }

    public static UserDto User { get; }
    public static ClientDto Client { get; }
}