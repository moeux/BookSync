using System.Text.Json.Serialization;

namespace BookSync.Models;

public class Book
{
    [JsonPropertyName("fast_hash")] public required string FastHash { get; init; }
    [JsonPropertyName("read_status")] public required string ReadStatus { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("read_percent")] public int ReadPercent { get; init; }
}