namespace Dais.Core.Domain.Interfaces;

public interface IAuthorizationCodeStore
{
    Task StoreAsync(AuthorizationCode code, CancellationToken ct = default);
    Task<AuthorizationCode?> FindAsync(string code, CancellationToken ct = default);
    Task ConsumeAsync(string code, CancellationToken ct = default);
}