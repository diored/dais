using DioRed.Dais.Core.Internal.Dto;

namespace DioRed.Dais.Core.Internal.Repositories;

internal interface IUserRepository
{
    string Add(string userName, string displayName, string passwordHash, string salt);
    UserDto? FindByUserName(string userName);
}