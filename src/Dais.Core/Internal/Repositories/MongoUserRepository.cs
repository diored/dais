
using DioRed.Dais.Core.Internal.Dto;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Internal.Repositories;

internal class MongoUserRepository(
    IMongoDatabase db,
    string collectionName
) : IUserRepository
{
    private readonly IMongoCollection<UserDto> _usersCollection = db.GetCollection<UserDto>(collectionName);

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

    public UserDto? FindByUserName(string userName)
    {
        var filter = Builders<UserDto>.Filter.Eq(x => x.UserName, userName);

        return _usersCollection.Find(filter).FirstOrDefault();
    }

    public UserDto? FindByUserId(string id)
    {
        var filter = Builders<UserDto>.Filter.Eq(x => x.Id, id);

        return _usersCollection.Find(filter).FirstOrDefault();
    }
}