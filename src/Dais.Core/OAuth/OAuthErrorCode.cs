namespace Dais.Core.OAuth;

public enum OAuthErrorCode
{
    InvalidRequest,
    InvalidClient,
    UnauthorizedClient,
    AccessDenied,
    UnsupportedResponseType,
    InvalidScope,
    ServerError,
    TemporarilyUnavailable,
    UnsupportedGrantType,
    InvalidGrant,
    InvalidRedirectUri,
    InvalidCodeChallenge,
    InvalidCodeVerifier,
    InvalidToken,
    InsufficientScope,
    InteractionRequired
}