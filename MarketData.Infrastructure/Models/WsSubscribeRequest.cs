using System.Text.Json.Serialization;

namespace MarketData.Infrastructure.Models
{
    public record WsSubscribeRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = "l1-subscription";

        [JsonPropertyName("id")]
        public string Id { get; init; } = "1";

        [JsonPropertyName("instrumentId")]
        public string InstrumentId { get; init; } = string.Empty;

        [JsonPropertyName("provider")]
        public string Provider { get; init; } = "oanda";

        [JsonPropertyName("subscribe")]
        public bool Subscribe { get; init; } = true;

        [JsonPropertyName("kinds")]
        public List<string> Kinds { get; init; } = new() { "ask", "bid", "last" };
    }
}
