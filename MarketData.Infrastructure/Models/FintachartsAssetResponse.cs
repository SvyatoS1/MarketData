using System.Text.Json.Serialization;

namespace MarketData.Infrastructure.Models
{
    public record FintachartsAssetResponse
    {
        [JsonPropertyName("data")]
        public List<FintachartsAssetDto>? Data { get; init; }
    }
}
