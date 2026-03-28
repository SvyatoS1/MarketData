using MarketData.Domain.Entities;

namespace MarketData.Application.Interfaces
{
    public interface IFintachartsWsClient
    {
        Task ConnectAndListenAsync(
            string accessToken,
            IEnumerable<string> instrumentIds,
            Func<AssetPrice, Task> onPriceReceived,
            CancellationToken cancellationToken);
    }
}
