using System.Text.Json.Serialization;

namespace MarketData.Infrastructure.Models
{
    public record WsPriceDetail
    {
        [JsonPropertyName("price")]
        public decimal Price { get; init; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; init; }
    }
}
