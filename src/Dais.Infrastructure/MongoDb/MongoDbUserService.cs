using System.Security.Cryptography;

using Dais.Core;
using Dais.Core.Domain;
using Dais.Core.Domain.Interfaces;
using Dais.Core.Internal;
using Dais.Core.Security;
using Dais.Infrastructure.MongoDb.Dto;

using MongoDB.Driver;

namespace Dais.Infrastructure.MongoDb;

public class MongoDbUserService(
    IMongoDatabase db,
    string collectionName,
    IPasswordHasher passwordHasher
) : IUserService
{
    private readonly IMongoCollection<UserDto> _usersCollection = db.GetCollection<UserDto>(collectionName);

    private readonly UserDto _dummyUser = new()
    {
        Id = IdGenerator.Generate(),
        UserName = "dummy_user",
        DisplayName = "Dummy User",
        PasswordHash = passwordHasher.Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(36)))
    };

    public string Add(string userName, string displayName, string password)
    {
        UserDto user = new()
        {
            Id = IdGenerator.Generate(),
            UserName = userName,
            DisplayName = displayName,
            PasswordHash = passwordHasher.Hash(password)
        };

        _usersCollection.InsertOne(user);

        return user.Id;
    }

    public UserProfile? GetByUsername(string userName)
    {
        var filter = Builders<UserDto>.Filter.Eq(x => x.UserName, userName);

        UserDto? dto = _usersCollection.Find(filter).FirstOrDefault();

        if (dto is null)
        {
            return null;
        }

        return new UserProfile
        {
            UserName = dto.UserName,
            DisplayName = dto.DisplayName
        };
    }

    public UserInfo? ValidateCredentials(string userName, string password)
    {
        var filter = Builders<UserDto>.Filter.Eq(x => x.UserName, userName);

        // Preventing the timing attacks
        UserDto? dto = _usersCollection.Find(filter).FirstOrDefault() ?? _dummyUser;

        if (!passwordHasher.Verify(password, dto.PasswordHash) || dto == _dummyUser)
        {
            return null;
        }

        return new UserInfo(dto.UserName, dto.DisplayName);
    }
}