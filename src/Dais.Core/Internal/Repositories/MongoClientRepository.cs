using DioRed.Dais.Core.Internal;
using DioRed.Dais.Core.Internal.Dto;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Internal.Repositories;

internal class MongoClientRepository(
    IMongoDatabase db,
    string collectionName
) : IClientRepository
{
    private readonly IMongoCollection<ClientDto> _clientsCollection = db.GetCollection<ClientDto>(collectionName);

    public string Add(string clientId, string clientSecret)
    {
        string id = IdGenerator.Generate();

        SaltedPassword saltedPassword = SaltedPassword.CreateWithRandomSalt(clientSecret);

        ClientDto dto = new()
        {
            Id = id,
            ClientId = clientId,
            Secret = saltedPassword.PasswordHash,
            Salt = saltedPassword.Salt
        };

        _clientsCollection.InsertOne(dto);

        return id;
    }

    public ClientDto? FindByClientId(string clientId)
    {
        return _clientsCollection.AsQueryable().FirstOrDefault(x => x.ClientId == clientId);
    }

    public ClientDto? Find(string clientId, string clientSecret)
    {
        return _clientsCollection.AsQueryable().FirstOrDefault(x => x.ClientId == clientId && x.Secret == clientSecret);
    }
}