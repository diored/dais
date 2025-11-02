using DioRed.Dais.Core.Dto;
using DioRed.Dais.Core.Entities;
using DioRed.Dais.Core.Repositories;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Services;

public class MongoDbDataService : IDataService
{
    private readonly MongoApplicationRepository _applications;
    private readonly MongoClientRepository _clients;
    private readonly MongoUserRepository _users;

    public MongoDbDataService(MongoDbSettings settings)
    {
        MongoClient mongoClient = new($"{settings.ConnectionString}/?authSource={settings.DatabaseName}");
        IMongoDatabase database = mongoClient.GetDatabase(settings.DatabaseName);

        _applications = new MongoApplicationRepository(database, settings.Collections.Applications);
        _clients = new MongoClientRepository(database, settings.Collections.Clients);
        _users = new MongoUserRepository(database, settings.Collections.Users);
    }

    public RegisteredApplication? FindApplicationByCallback(string redirectUri, string clientId)
    {
        if (_clients.FindByClientId(clientId) is not { } owner)
        {
            return null;
        }

        if (_applications.Find(owner.Id, redirectUri) is not { } application)
        {
            return null;
        }

        return new RegisteredApplication { ApplicationName = application.Name };
    }

    public UserProfile? FindUser(string username, string password)
    {
        // Preventing the timing attacks
        UserDto user = _users.FindByUserName(username) ?? DummyUser.Instance;

        byte[] salt = Convert.FromBase64String(user.Salt);
        SaltedPassword saltedPassword = SaltedPassword.Create(password, salt);

        if (saltedPassword.PasswordHash != user.Password || user == DummyUser.Instance)
        {
            return null;
        }

        return new UserProfile
        {
            UserName = user.UserName,
            DisplayName = user.DisplayName
        };
    }

    public bool HasRegisteredClient(string clientId)
    {
        return _clients.FindByClientId(clientId) is not null;
    }

    public bool HasRegisteredClient(string clientId, string clientSecret)
    {
        return _clients.Find(clientId, clientSecret) is not null;
    }

    public void RegisterApp(string clientId, string appName, string[] callbacks)
    {
        if (_clients.FindByClientId(clientId) is not { } client)
        {
            throw new InvalidOperationException("Client not found");
        }

        _applications.Add(client.Id, appName, callbacks);
    }

    public void RegisterClient(string clientId, string clientSecret)
    {
        _clients.Add(clientId, clientSecret);
    }

    public void RegisterUser(string userName, string displayName, string password)
    {
        SaltedPassword salted = SaltedPassword.CreateWithRandomSalt(password);

        _users.Add(userName, displayName, salted.PasswordHash, salted.Salt);
    }
}