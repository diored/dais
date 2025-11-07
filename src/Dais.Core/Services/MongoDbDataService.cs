using DioRed.Dais.Core.Entities;
using DioRed.Dais.Core.Internal;
using DioRed.Dais.Core.Internal.Dto;
using DioRed.Dais.Core.Internal.Repositories;

using Isopoh.Cryptography.Argon2;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Services;

public class MongoDbDataService : IDataService
{
    private readonly MongoClientRepository _clients;
    private readonly MongoUserRepository _users;

    public MongoDbDataService(MongoDbSettings settings)
    {
        MongoClient mongoClient = new($"{settings.ConnectionString}/?authSource={settings.DatabaseName}");
        IMongoDatabase database = mongoClient.GetDatabase(settings.DatabaseName);

        _clients = new MongoClientRepository(database, settings.Collections.Clients);
        _users = new MongoUserRepository(database, settings.Collections.Users);
    }

    public RegisteredClient? FindClient(string clientId)
    {
        ClientDto? client = _clients.FindByClientId(clientId);

        return client is null
            ? null
            : new RegisteredClient { DisplayName = client.DisplayName };
    }

    public RegisteredClientWithCallbacks? FindClient(string clientId, string clientSecret)
    {
        // Preventing the timing attacks
        ClientDto client = _clients.FindByClientId(clientId) ?? Dummy.Client;

        if (!Argon2.Verify(client.ClientSecretHash, clientSecret) || client == Dummy.Client)
        {
            return null;
        }

        return new RegisteredClientWithCallbacks
        {
            DisplayName = client.DisplayName,
            Callbacks = client.Callbacks
        };
    }

    public UserProfile? FindUser(string username, string password)
    {
        // Preventing the timing attacks
        UserDto user = _users.FindByUserName(username) ?? Dummy.User;

        if (!Argon2.Verify(user.PasswordHash, password) || user == Dummy.User)
        {
            return null;
        }

        return new UserProfile
        {
            UserName = user.UserName,
            DisplayName = user.DisplayName
        };
    }

    public void RegisterClient(string ownerId, string clientId, string clientSecret, string displayName, string[] callbacks)
    {
        if (_users.FindByUserId(ownerId) is null)
        {
            throw new InvalidOperationException($"Owner (id={ownerId}) is not registered.");
        }

        if (_clients.FindByClientId(clientId) is not null)
        {
            throw new InvalidOperationException("Client already registered");
        }

        _clients.Add(ownerId, clientId, clientSecret, displayName, callbacks);
    }

    public void RegisterUser(string userName, string displayName, string password)
    {
        _users.Add(
            userName,
            displayName,
            password
        );
    }
}