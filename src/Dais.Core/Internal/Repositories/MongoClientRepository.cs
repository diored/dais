using DioRed.Dais.Core.Internal.Dto;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Internal.Repositories;

internal class MongoClientRepository(
    IMongoDatabase db,
    string collectionName
) : IClientRepository
{
    private readonly IMongoCollection<ClientDto> _clientsCollection = db.GetCollection<ClientDto>(collectionName);

    public string Add(
        string ownerId,
        string clientId,
        string clientSecret,
        string displayName,
        string[] callbacks
    )
    {
        string id = IdGenerator.Generate();

        SaltedPassword saltedPassword = SaltedPassword.CreateWithRandomSalt(clientSecret);

        ClientDto dto = new()
        {
            Id = id,
            OwnerId = ownerId,
            ClientId = clientId,
            ClientSecret = saltedPassword.PasswordHash,
            Salt = saltedPassword.Salt,
            DisplayName = displayName,
            Callbacks = callbacks
        };

        _clientsCollection.InsertOne(dto);

        return id;
    }

    public ClientDto? FindByClientId(string clientId)
    {
        var filter = Builders<ClientDto>.Filter.Eq(x => x.ClientId, clientId);

        return _clientsCollection.Find(filter).FirstOrDefault();
    }
}