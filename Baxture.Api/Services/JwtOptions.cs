namespace Baxture.Api.Services;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = "baxture-local-development-secret-key-change-me";
    public string Issuer { get; set; } = "Baxture.Api";
    public string Audience { get; set; } = "Baxture.Api.Users";
    public int ExpirationMinutes { get; set; } = 60;
}
