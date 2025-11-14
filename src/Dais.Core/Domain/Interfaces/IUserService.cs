namespace Dais.Core.Domain.Interfaces;

public record UserInfo(string UserName, string DisplayName);

public interface IUserService
{
    UserProfile? GetByUsername(string userName);
    UserInfo? ValidateCredentials(string userName, string password);
}