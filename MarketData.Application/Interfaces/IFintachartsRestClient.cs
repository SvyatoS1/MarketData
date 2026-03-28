using MarketData.Domain.Entities;

namespace MarketData.Application.Interfaces
{
    public interface IFintachartsRestClient
    {
        Task<IEnumerable<Asset>> GetSupportedAssetsAsync(CancellationToken cancellationToken = default);
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
