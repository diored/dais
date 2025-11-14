namespace Dais.Core.Domain.Interfaces;

public interface IClientService
{
    RegisteredClient? FindById(string clientId);
}