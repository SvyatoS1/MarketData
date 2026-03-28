using MarketData.Domain.Entities;
using MarketData.Domain.Interfaces;
using MarketData.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Infrastructure.Repositories
{
    public class AssetRepository : IAssetRepository
    {

        private readonly MarketDbContext _context;
        public AssetRepository(MarketDbContext context)
        {
            _context = context;
        }
        public async Task AddAssetsAsync(IEnumerable<Asset> assets, CancellationToken cancellationToken = default)
        {
            var existingSymbols = await _context.Assets
                .Select(a => a.Symbol)
                .ToListAsync(cancellationToken);

            var newAssets = assets.Where(a => !existingSymbols.Contains(a.Symbol));

            if (newAssets.Any())
            {
                await _context.Assets
                    .AddRangeAsync(newAssets, cancellationToken);
            }
        }

        public async Task AddPriceAsync(AssetPrice price, CancellationToken cancellationToken = default)
        {
            await _context.AssetPrices
                .AddAsync(price, cancellationToken);
        }

        public async Task<IEnumerable<Asset>> GetAllSupportedAssetsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Assets
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Asset?> GetAssetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
        {
            return await _context.Assets
                .FirstOrDefaultAsync(a => a.Symbol == symbol, cancellationToken);
        }

        public async Task<AssetPrice?> GetLatestPriceAsync(string symbol, CancellationToken cancellationToken = default)
        {
            return await _context.AssetPrices
                .Where(p => p.AssetSymbol == symbol)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
