using MarketData.Api.Models;
using MarketData.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MarketData.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetRepository _repository;

        public AssetsController(IAssetRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssets(CancellationToken cancellationToken)
        {
            var assets = await _repository.GetAllSupportedAssetsAsync(cancellationToken);

            if (!assets.Any())
            {
                return NoContent();
            }

            var response = assets.Select(a => new AssetResponse(a.Symbol, a.Description));

            return Ok(response);
        }

        [HttpGet("{symbol}/price")]
        public async Task<IActionResult> GetAssetPrice(string symbol, CancellationToken cancellationToken)
        {
            var asset = await _repository.GetAssetBySymbolAsync(symbol, cancellationToken);
            if (asset == null)
            {
                return NotFound(new { Message = $"Asset '{symbol}' not supported." });
            }

            var latestPrice = await _repository.GetLatestPriceAsync(symbol, cancellationToken);

            if (latestPrice == null)
            {
                return NotFound(new { Message = $"Price for '{symbol}' cant retrive. Check later." });
            }

            var response = new AssetPriceResponse(
                latestPrice.AssetSymbol,
                latestPrice.Price,
                latestPrice.Timestamp);

            return Ok(response);
        }
    }
}
