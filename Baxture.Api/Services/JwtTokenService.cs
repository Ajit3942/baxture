using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Baxture.Api.Models;
using Microsoft.Extensions.Options;

namespace Baxture.Api.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public string CreateToken(User user)
    {
        var now = DateTimeOffset.UtcNow;
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["sub"] = user.Id,
            ["username"] = user.Username,
            ["isAdmin"] = user.IsAdmin,
            ["iss"] = _options.Issuer,
            ["aud"] = _options.Audience,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddMinutes(_options.ExpirationMinutes).ToUnixTimeSeconds()
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signature = Sign($"{encodedHeader}.{encodedPayload}");

        return $"{encodedHeader}.{encodedPayload}.{signature}";
    }

    public AuthenticatedUser? ValidateToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var signature = Sign($"{parts[0]}.{parts[1]}");
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(signature),
                Encoding.ASCII.GetBytes(parts[2])))
        {
            return null;
        }

        using var payloadDocument = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var payload = payloadDocument.RootElement;

        if (!StringClaimEquals(payload, "iss", _options.Issuer) ||
            !StringClaimEquals(payload, "aud", _options.Audience))
        {
            return null;
        }

        if (!payload.TryGetProperty("exp", out var exp) ||
            DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var id = payload.GetProperty("sub").GetString();
        var username = payload.GetProperty("username").GetString();
        var isAdmin = payload.GetProperty("isAdmin").GetBoolean();

        return string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(username)
            ? null
            : new AuthenticatedUser(id, username, isAdmin);
    }

    private string Sign(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.Secret));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));
    }

    private static bool StringClaimEquals(JsonElement payload, string name, string expected) =>
        payload.TryGetProperty(name, out var claim) && claim.GetString() == expected;

    private static string Base64UrlEncode(byte[] value) =>
        Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }
}
