using DioRed.Dais.Core.Internal.Dto;

namespace DioRed.Dais.Core.Internal.Repositories;

internal interface IApplicationRepository
{
    string Add(string ownerId, string name, string[] callbacks);
    ApplicationDto? Find(string ownerId, string callback);
}
