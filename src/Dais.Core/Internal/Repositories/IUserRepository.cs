using DioRed.Dais.Core.Internal.Dto;

namespace DioRed.Dais.Core.Internal.Repositories;

internal interface IUserRepository
{
    UserDto? FindByUserName(string userName);
}
