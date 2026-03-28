namespace MarketData.Domain.Entities
{
    public class AssetPrice
    {
        public int Id { get; set; }
        public string AssetSymbol { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime Timestamp { get; set; }

        public Asset? Asset { get; set; }
    }
}
