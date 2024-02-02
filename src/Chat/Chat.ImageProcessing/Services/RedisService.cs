using System.Text.Json;
using Chat.Observability.Options;
using StackExchange.Redis;

namespace Chat.ImageProcessing.Services;

public class RedisService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public RedisService(MicroServiceOptions options)
    {
        _redis = ConnectionMultiplexer.Connect(options.Redis);
        _database = _redis.GetDatabase();
    }

    public async Task SetAsync<T>(string key, T item, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(item);
        await _database.StringSetAsync(key, json, expiry);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var json = await _database.StringGetAsync(key);
        return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
    }
}
