using System.Security.Cryptography;

using DioRed.Dais.Core.Dto;

namespace DioRed.Dais.Core;

/// <summary>
/// Helps to prevent timing attacks
/// </summary>
internal static class DummyUser
{
    private static readonly Lazy<UserDto> _instance = new(() =>
    {
        string randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(36));

        SaltedPassword saltedPassword = SaltedPassword.CreateWithRandomSalt(randomPassword);

        return new UserDto
        {
            Id = IdGenerator.Generate(),
            UserName = "dummy_user",
            DisplayName = "Dummy User",
            Password = saltedPassword.PasswordHash,
            Salt = saltedPassword.Salt
        };
    });

    public static UserDto Instance => _instance.Value;
}