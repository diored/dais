using DioRed.Dais.Core.Entities;

namespace DioRed.Dais.Core.Services;

public interface IDataService
{
    RegisteredApplication? FindApplicationByCallback(string callback, string clientId);
    UserProfile? FindUser(string username, string password);
    bool HasRegisteredClient(string clientId);
    bool HasRegisteredClient(string clientId, string clientSecret);
    void RegisterApp(string clientId, string appName, string[] callbacks);
    void RegisterClient(string clientId, string clientSecret);
    void RegisterUser(string userName, string displayName, string password);
}