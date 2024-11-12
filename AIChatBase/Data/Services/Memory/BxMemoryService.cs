using Microsoft.Extensions.Caching.Memory;

namespace AIChatBase.Data.Services.Memory;

/// <summary>
/// 内存缓存封装
/// </summary>
public class BxMemoryService : IBxMemoryService
{
    /// <summary>
    /// 默认过期时间
    /// </summary>
    private static readonly TimeSpan defaultTimeSpan = TimeSpan.FromMinutes(10);
    /// <summary>
    /// 默认前缀
    /// </summary>
    private static readonly string baseKey = "Memory_";
    /// <summary>
    /// 系统Memory
    /// </summary>
    private readonly IMemoryCache _memory;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="memory"></param>
    public BxMemoryService(IMemoryCache memory)
    {
        _memory = memory;
    }
    /// <summary>
    /// 异步缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public async Task<T> GetOrCreateAsync<T>(object key, Func<Task<T>> func, TimeSpan? expr = null)
    {
        var result = await _memory.GetOrCreateAsync($"{baseKey}{key}", async (entity) =>
        {
            entity.AbsoluteExpirationRelativeToNow = expr ?? defaultTimeSpan;
            return await func();
        });
        return result;
    }

    /// <summary>
    /// 同步缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public T GetOrCreate<T>(object key, Func<T> func, TimeSpan? expr = null)
    {
        var result = _memory.GetOrCreate($"{baseKey}{key}", (entity) =>
        {
            entity.AbsoluteExpirationRelativeToNow = expr ?? defaultTimeSpan;
            return func();
        });
        return result;
    }
}