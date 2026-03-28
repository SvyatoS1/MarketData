using MarketData.Application.Interfaces;
using MarketData.Domain.Entities;
using MarketData.Infrastructure.Configuration;
using MarketData.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MarketData.Infrastructure.FintachartsApi
{
    public class FintachartsWsClient : IFintachartsWsClient
    {
        private const int BufferSize = 8192;
        private const int SubscriptionDelayMs = 100;

        private readonly FintachartsSettings _settings;
        private readonly ILogger<FintachartsWsClient> _logger;
        private int _requestIdCounter = 1;

        public FintachartsWsClient(
            IOptions<FintachartsSettings> settings,
            ILogger<FintachartsWsClient> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task ConnectAndListenAsync(
            string accessToken,
            IEnumerable<string> instrumentIds,
            Func<AssetPrice, Task> onPriceReceived,
            CancellationToken cancellationToken)
        {
            using var ws = new ClientWebSocket();

            var wsUri = new Uri($"{_settings.WsApiBaseUrl}?token={accessToken}");

            try
            {
                await ws.ConnectAsync(wsUri, cancellationToken);
                _logger.LogInformation("Successfully connected to WebSocket.");

                foreach (var instrumentId in instrumentIds)
                {
                    await SubscribeAsync(ws, instrumentId, cancellationToken);
                    await Task.Delay(SubscriptionDelayMs, cancellationToken);
                }

                await ReceiveLoopAsync(ws, onPriceReceived, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebSocket error.");
            }
        }

        private async Task SubscribeAsync(
            ClientWebSocket ws,
            string instrumentId,
            CancellationToken cancellationToken)
        {
            var request = new WsSubscribeRequest
            {
                Id = (_requestIdCounter++).ToString(),
                InstrumentId = instrumentId
            };

            var json = JsonSerializer.Serialize(request);
            var bytes = Encoding.UTF8.GetBytes(json);

            await ws.SendAsync(
                bytes,
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);

            _logger.LogInformation("Subscribed to {InstrumentId}", instrumentId);
        }

        private async Task ReceiveLoopAsync(
            ClientWebSocket ws,
            Func<AssetPrice, Task> onPriceReceived,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[BufferSize];

            while (ws.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning(
                        "WebSocket closed: {Status} - {Description}",
                        ws.CloseStatus,
                        ws.CloseStatusDescription);

                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                try
                {
                    var update = JsonSerializer.Deserialize<WsPriceUpdateMessage>(message);

                    if (update?.Last == null)
                        continue;

                    var assetPrice = new AssetPrice
                    {
                        AssetSymbol = update.InstrumentId,
                        Price = update.Last.Price,
                        Timestamp = update.Last.Timestamp
                    };

                    await onPriceReceived(assetPrice);
                }
                catch (JsonException)
                {
                    _logger.LogDebug("Invalid message received: {Message}", message);
                }
            }
        }
    }
}