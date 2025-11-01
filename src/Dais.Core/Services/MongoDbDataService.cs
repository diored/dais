using DioRed.Dais.Core.Dto;
using DioRed.Dais.Core.Entities;
using DioRed.Dais.Core.Repositories;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Services;

public class MongoDbDataService(
    IMongoDatabase db,
    string applicationsCollectionName,
    string clientsCollectionName,
    string usersCollectionName
) : IDataService
{
    private readonly MongoApplicationRepository applications = new(db, applicationsCollectionName);
    private readonly MongoClientRepository clients = new(db, clientsCollectionName);
    private readonly MongoUserRepository users = new(db, usersCollectionName);

    public RegisteredApplication? FindApplicationByCallback(string redirectUri, string clientId)
    {
        if (clients.FindByClientId(clientId) is not { } owner)
        {
            return null;
        }

        if (applications.Find(owner.Id, redirectUri) is not { } application)
        {
            return null;
        }

        return new RegisteredApplication { ApplicationName = application.Name };
    }

    public UserProfile? FindUser(string username, string password)
    {
        // Preventing the timing attacks
        UserDto user = users.FindByUserName(username) ?? DummyUser.Instance;

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
        return clients.FindByClientId(clientId) is not null;
    }

    public bool HasRegisteredClient(string clientId, string clientSecret)
    {
        return clients.Find(clientId, clientSecret) is not null;
    }

    public void RegisterApp(string clientId, string appName, string[] callbacks)
    {
        if (clients.FindByClientId(clientId) is not { } client)
        {
            throw new InvalidOperationException("Client not found");
        }

        applications.Add(client.Id, appName, callbacks);
    }

    public void RegisterClient(string clientId, string clientSecret)
    {
        clients.Add(clientId, clientSecret);
    }

    public void RegisterUser(string userName, string displayName, string password)
    {
        SaltedPassword salted = SaltedPassword.CreateWithRandomSalt(password);

        users.Add(userName, displayName, salted.PasswordHash, salted.Salt);
    }
}