using System.Text.Json.Serialization;

namespace BookSync.Models;

public class Login
{
    private readonly DateTimeOffset _retrieved = DateTime.UtcNow;
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("token_type")] public string? TokenType { get; init; }
    [JsonPropertyName("expires_in")] public long ExpiresIn { get; init; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }

    public bool IsExpired => string.IsNullOrWhiteSpace(AccessToken) ||
                             _retrieved.AddSeconds(ExpiresIn) <= DateTimeOffset.UtcNow;
}