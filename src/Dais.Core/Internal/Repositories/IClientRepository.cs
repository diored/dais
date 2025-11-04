using DioRed.Dais.Core.Internal.Dto;

namespace DioRed.Dais.Core.Internal.Repositories;

internal interface IClientRepository
{
    string Add(string ownerId, string clientId, string clientSecret, string displayName, string[] callbacks);
    ClientDto? FindByClientId(string clientId);
}