using MarketData.Domain.Entities;

namespace MarketData.Domain.Interfaces
{
    public interface IAssetRepository
    {
        Task<IEnumerable<Asset>> GetAllSupportedAssetsAsync(CancellationToken cancellationToken = default);
        Task<Asset?> GetAssetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
        Task AddAssetsAsync(IEnumerable<Asset> assets, CancellationToken cancellationToken = default);

        Task<AssetPrice?> GetLatestPriceAsync(string symbol, CancellationToken cancellationToken = default);
        Task AddPriceAsync(AssetPrice price, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
