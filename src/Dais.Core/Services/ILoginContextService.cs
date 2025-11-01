using DioRed.Dais.Core.Entities;

namespace DioRed.Dais.Core.Services;

public interface ILoginContextService
{
    LoginContext? Pull(string key);
    string Save(LoginContext loginContext);
}