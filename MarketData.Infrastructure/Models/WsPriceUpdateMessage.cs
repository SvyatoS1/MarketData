using System.Text.Json.Serialization;

namespace MarketData.Infrastructure.Models
{
    public record WsPriceUpdateMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("instrumentId")]
        public string InstrumentId { get; init; } = string.Empty;

        [JsonPropertyName("last")]
        public WsPriceDetail? Last { get; init; }
    }
}
