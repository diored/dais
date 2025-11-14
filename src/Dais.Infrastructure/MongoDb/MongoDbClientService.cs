using Dais.Core;
using Dais.Core.Domain;
using Dais.Core.Domain.Interfaces;
using Dais.Core.Internal;
using Dais.Core.Security;
using Dais.Infrastructure.MongoDb.Dto;

using MongoDB.Driver;

namespace Dais.Infrastructure.MongoDb;

public class MongoDbClientService(
    IMongoDatabase db,
    string collectionName,
    IPasswordHasher passwordHasher
) : IClientService
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

        ClientDto dto = new()
        {
            Id = id,
            OwnerId = ownerId,
            ClientId = clientId,
            ClientSecretHash = passwordHasher.Hash(clientSecret),
            DisplayName = displayName,
            Callbacks = callbacks
        };

        _clientsCollection.InsertOne(dto);

        return id;
    }

    public RegisteredClient? FindById(string clientId)
    {
        var filter = Builders<ClientDto>.Filter.Eq(x => x.ClientId, clientId);

        ClientDto? dto = _clientsCollection.Find(filter).FirstOrDefault();

        if (dto is null)
        {
            return null;
        }

        bool isPublic = string.IsNullOrEmpty(dto.ClientSecretHash);

        return new RegisteredClient
        {
            ClientId = dto.ClientId,
            IsPublic = isPublic,
            RequirePkce = isPublic,
            SecretHash = dto.ClientSecretHash,
            RedirectUris = [.. dto.Callbacks],
            AllowedGrantTypes = [.. _allowedGrantTypes],
            AllowedScopes = [.. _allowedScopes]
        };
    }

    private static readonly string[] _allowedGrantTypes =
    [
        "authorization_code",
        "refresh_token",
        "client_credentials"
    ];

    private static readonly string[] _allowedScopes =
    [
        "openid",
        "profile",
        "email",
        "offline_access"
    ];
}