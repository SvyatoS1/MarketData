namespace MarketData.Domain.Entities
{
    public class Asset
    {
        public string Symbol { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<AssetPrice> Prices { get; set; } = new List<AssetPrice>();
    }
}
