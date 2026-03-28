using MarketData.Application.Interfaces;
using MarketData.Domain.Entities;
using MarketData.Infrastructure.Configuration;
using MarketData.Infrastructure.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MarketData.Infrastructure.FintachartsApi
{
    public class FintachartsRestClient : IFintachartsRestClient
    {
        private readonly HttpClient _httpClient;
        private readonly FintachartsSettings _settings;
        private readonly IMemoryCache _cache;

        private const string AccessTokenCacheKey = "Fintacharts_Default_AccessToken";
        private const string RefreshTokenCacheKey = "Fintacharts_Default_RefreshToken";
        public FintachartsRestClient(HttpClient httpClient,
            IOptions<FintachartsSettings> settings,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _cache = cache;
        }

        public async Task<IEnumerable<Asset>> GetSupportedAssetsAsync(CancellationToken cancellationToken = default)
        {
            var token = await GetAccessTokenAsync(cancellationToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var endpoint = "/api/instruments/v1/instruments?provider=oanda&kind=forex";

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<FintachartsAssetResponse>(cancellationToken: cancellationToken);

            if (result?.Data == null)
            {
                return Enumerable.Empty<Asset>();
            }

            return result.Data.Select(dto => new Asset
            {
                Id = dto.Id,
                Symbol = dto.Symbol,
                Description = dto.Description ?? string.Empty,
            });
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(AccessTokenCacheKey, out string? cachedAccessToken) && !string.IsNullOrEmpty(cachedAccessToken))
            {
                return cachedAccessToken;
            }

            if (_cache.TryGetValue(RefreshTokenCacheKey, out string? cachedRefreshToken) && !string.IsNullOrEmpty(cachedRefreshToken))
            {
                try
                {
                    return await RefreshTokenAsync(cachedRefreshToken, cancellationToken);
                }
                catch (Exception)
                {
                    _cache.Remove(RefreshTokenCacheKey);
                }
            }
            return await AuthenticateWithPasswordAsync(cancellationToken);
        }

        private async Task<string> AuthenticateWithPasswordAsync(CancellationToken cancellationToken)
        {
            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _settings.ClientId },
                { "username", _settings.Username },
                { "password", _settings.Password }
            };

            return await ExecuteAuthRequestAsync(requestData, cancellationToken);
        }

        private async Task<string> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", _settings.ClientId },
                { "refresh_token", refreshToken }
            };

            return await ExecuteAuthRequestAsync(requestData, cancellationToken);
        }

        private async Task<string> ExecuteAuthRequestAsync(Dictionary<string, string> requestData, CancellationToken cancellationToken)
        {
            var authEndpoint = $"/identity/realms/{_settings.Realm}/protocol/openid-connect/token";
            var content = new FormUrlEncodedContent(requestData);

            var response = await _httpClient.PostAsync(authEndpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<FintachartsTokenResponse>(cancellationToken: cancellationToken);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new Exception("Cant get token from Fintacharts.");
            }

            var accessTokenExpiration = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 10);
            _cache.Set(AccessTokenCacheKey, tokenResponse.AccessToken, accessTokenExpiration);

            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken) && tokenResponse.RefreshExpiresIn > 0)
            {
                var refreshTokenExpiration = TimeSpan.FromSeconds(tokenResponse.RefreshExpiresIn - 10);
                _cache.Set(RefreshTokenCacheKey, tokenResponse.RefreshToken, refreshTokenExpiration);
            }

            return tokenResponse.AccessToken;
        }
    }
}
