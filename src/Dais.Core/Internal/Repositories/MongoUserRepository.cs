using DioRed.Dais.Core.Internal;
using DioRed.Dais.Core.Internal.Dto;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Internal.Repositories;

internal class MongoUserRepository(
    IMongoDatabase db,
    string collectionName
) : IUserRepository
{
    private readonly IMongoCollection<UserDto> _usersCollection = db.GetCollection<UserDto>(collectionName);

    public UserDto? FindByUserName(string userName)
    {
        return _usersCollection.AsQueryable().FirstOrDefault(x => x.UserName == userName);
    }

    public string Add(string userName, string displayName, string passwordHash, string salt)
    {
        UserDto user = new()
        {
            Id = IdGenerator.Generate(),
            UserName = userName,
            DisplayName = displayName,
            Password = passwordHash,
            Salt = salt
        };

        _usersCollection.InsertOne(user);

        return user.Id;
    }
}