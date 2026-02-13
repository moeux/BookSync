using System.Text.Json.Serialization;

namespace BookSync.Models;

public class Provider
{
    [JsonPropertyName("alias")] public required string Alias { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("shop_id")] public required string ShopId { get; init; }
    [JsonPropertyName("logged_by")] public required string LoggedBy { get; init; }
}