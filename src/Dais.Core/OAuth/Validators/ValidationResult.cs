namespace Dais.Core.OAuth.Validators;

public sealed class ValidationResult
{
    public bool IsValid { get; }
    public OAuthErrorCode? ErrorCode { get; }
    public string? Description { get; }
    public string? State { get; }

    private ValidationResult(bool ok, OAuthErrorCode? code = null, string? desc = null, string? state = null)
        => (IsValid, ErrorCode, Description, State) = (ok, code, desc, state);

    public static ValidationResult Valid() => new(true);
    public static ValidationResult Invalid(OAuthErrorCode code, string desc, string? state = null) => new(false, code, desc, state);
}