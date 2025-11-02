using DioRed.Dais.Core.Internal.Dto;

namespace DioRed.Dais.Core.Internal.Repositories;

internal interface IClientRepository
{
    string Add(string clientId, string clientSecret);
    ClientDto? FindByClientId(string clientId);
}