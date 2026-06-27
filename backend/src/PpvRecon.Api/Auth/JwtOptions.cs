namespace PpvRecon.Api.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "PpvRecon";
    public string Audience { get; set; } = "PpvRecon.Web";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenHours { get; set; } = 5;
}
