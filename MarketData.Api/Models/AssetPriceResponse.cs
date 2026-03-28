namespace MarketData.Api.Models;

public record AssetPriceResponse(string Symbol, decimal Price, DateTime LastUpdated);
