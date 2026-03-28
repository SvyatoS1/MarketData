using System.Text.Json.Serialization;

namespace MarketData.Infrastructure.Models
{
    public record FintachartsAssetDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;
    }
}
