namespace MarketData.Infrastructure.Configuration
{
    public class FintachartsSettings
    {
        public string RestApiBaseUrl { get; set; } = string.Empty;
        public string WsApiBaseUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Realm { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
    }
}
