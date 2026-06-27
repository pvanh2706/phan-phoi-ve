using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PpvRecon.Application.Auth;

namespace PpvRecon.Api.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public AccessTokenResult GenerateAccessToken(
        int userId,
        string email,
        string fullName,
        string role,
        int sessionId,
        DateTime nowUtc)
    {
        EnsureSigningKeyIsValid();

        var jwtId = Guid.NewGuid().ToString("N");
        var expiresAtUtc = nowUtc.AddMinutes(_options.AccessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Role, role),
            new("session_id", sessionId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            jwtId);
    }

    public RefreshTokenResult GenerateRefreshToken(DateTime nowUtc)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = WebEncoders.Base64UrlEncode(bytes);
        return new RefreshTokenResult(
            refreshToken,
            HashRefreshToken(refreshToken),
            nowUtc.AddHours(_options.RefreshTokenHours));
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }

    private void EnsureSigningKeyIsValid()
    {
        if (string.IsNullOrWhiteSpace(_options.SigningKey) || Encoding.UTF8.GetByteCount(_options.SigningKey) < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey phải có ít nhất 32 ký tự.");
        }
    }
}
