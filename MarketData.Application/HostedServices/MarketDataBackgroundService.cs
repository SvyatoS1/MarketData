using MarketData.Application.Interfaces;
using MarketData.Domain.Entities;
using MarketData.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketData.Application.HostedServices
{
    public class MarketDataBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFintachartsWsClient _wsClient;
        private readonly ILogger<MarketDataBackgroundService> _logger;

        public MarketDataBackgroundService(
            IServiceProvider serviceProvider,
            IFintachartsWsClient wsClient,
            ILogger<MarketDataBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _wsClient = wsClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servie Market Data launched.");

            try
            {
                string accessToken;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var restClient = scope.ServiceProvider.GetRequiredService<IFintachartsRestClient>();
                    var repository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();

                    accessToken = await restClient.GetAccessTokenAsync(stoppingToken);

                    var assets = await restClient.GetSupportedAssetsAsync(stoppingToken);
                    var assetList = assets.ToList();

                    await repository.AddAssetsAsync(assetList, stoppingToken);
                    await repository.SaveChangesAsync(stoppingToken);

                    var targetAssetsDict = assetList.ToDictionary(a => a.Id, a => a.Symbol);

                    var targetInstrumentIds = targetAssetsDict.Keys.ToList();

                    _logger.LogInformation($"Loaded assets: {assetList.Count}. Subcribe on {targetInstrumentIds.Count}.");

                    await _wsClient.ConnectAndListenAsync(
                    accessToken,
                    targetInstrumentIds,
                    async (price) =>
                    {
                        if (targetAssetsDict.TryGetValue(price.AssetSymbol, out var correctSymbol))
                        {
                            price.AssetSymbol = correctSymbol;
                            await ProcessPriceUpdateAsync(price, stoppingToken);
                        }
                    },
                    stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error in background service.");
            }
        }

        private async Task ProcessPriceUpdateAsync(AssetPrice price, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();

                await repository.AddPriceAsync(price, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Updated price: {price.AssetSymbol} -> {price.Price} (Time: {price.Timestamp})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error for saving price {price.AssetSymbol}");
            }
        }
    }
}
