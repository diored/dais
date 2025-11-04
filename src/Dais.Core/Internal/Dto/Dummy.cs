using System.Security.Cryptography;

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
        SaltedPassword saltedPassword = SaltedPassword.CreateWithRandomSalt(randomPassword);

        User = new UserDto
        {
            Id = id,
            UserName = "dummy_user",
            DisplayName = "Dummy User",
            Password = saltedPassword.PasswordHash,
            Salt = saltedPassword.Salt
        };

        Client = new ClientDto
        {
            Id = id,
            OwnerId = "",
            ClientId = "dummy_client",
            ClientSecret = saltedPassword.PasswordHash,
            Salt = saltedPassword.Salt,
            DisplayName = "Dummy Client",
            Callbacks = []
        };
    }

    public static UserDto User { get; }
    public static ClientDto Client { get; }
}