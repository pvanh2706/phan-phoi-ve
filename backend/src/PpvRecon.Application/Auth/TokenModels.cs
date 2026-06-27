namespace PpvRecon.Application.Auth;

public sealed record AccessTokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string JwtId);

public sealed record RefreshTokenResult(
    string RefreshToken,
    string RefreshTokenHash,
    DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessTokenResult GenerateAccessToken(
        int userId,
        string email,
        string fullName,
        string role,
        int sessionId,
        DateTime nowUtc);

    RefreshTokenResult GenerateRefreshToken(DateTime nowUtc);
    string HashRefreshToken(string refreshToken);
}
