using DioRed.Dais.Core.Entities;

namespace DioRed.Dais.Core.Services;

public interface IDataService
{
    RegisteredClient? FindClient(string clientId);
    RegisteredClientWithCallbacks? FindClient(string clientId, string clientSecret);
    UserProfile? FindUser(string username, string password);
    void RegisterClient(string ownerId, string clientId, string clientSecret, string displayName, string[] callbacks);
    void RegisterUser(string userName, string displayName, string password);
}