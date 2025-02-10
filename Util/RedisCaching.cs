using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Util;

public class RedisCaching
{
    private const int DefaultAbsoluteExpirationMinutes = 30;
    private const int DefaultSlidingExpirationMinutes = 10;

    private readonly IDistributedCache _cache;
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _serializer;

    public RedisCaching(IDistributedCache cache, ILogger logger)
    {
        _cache = cache;
        _logger = logger;
        _serializer = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null)
    {
        ValidateKey(key);
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    absoluteExpiration ?? TimeSpan.FromMinutes(DefaultAbsoluteExpirationMinutes),
                SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(DefaultSlidingExpirationMinutes)
            };

            var serializedValue = JsonSerializer.Serialize(value, _serializer);
            await _cache.SetStringAsync(key, serializedValue, options);
            _logger.Information("Cache set for key: {Key}", key);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Failed to set cache for key: {Key}", key);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        ValidateKey(key);

        try
        {
            var cacheData = await _cache.GetStringAsync(key);
            if (cacheData == null)
            {
                _logger.Information("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.Information("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cacheData, _serializer);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Failed to get cache for key: {Key}", key);
            return default;
        }
    }

    public async Task RemoveAsync(string key)
    {
        ValidateKey(key);

        try
        {
            await _cache.RemoveAsync(key);
            _logger.Information("Cache removed for key: {Key}", key);
        }
        catch (System.Exception ex)
        {
            _logger.Error(ex, "Failed to remove cache for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        ValidateKey(key);
        return await _cache.GetStringAsync(key) != null;
    }
}