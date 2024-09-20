using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GPA.Utils.Caching
{
    public interface IGenericCache<TItem>
    {
        TItem GetOrCreate(CacheType cacheType, string key, Func<TItem> func);
        Task<TItem> GetOrCreate(CacheType cacheType, string key, Func<Task<TItem>> func);
        Task<IEnumerable<TItem>> GetOrCreate(CacheType cacheType, string key, Func<Task<IEnumerable<TItem>>> func);
        IEnumerable<TItem> GetOrCreate(CacheType cacheType, string key, Func<IEnumerable<TItem>> func);
    }

    public class GenericCache<TItem> : IGenericCache<TItem>
    {
        private static MemoryCacheEntryOptions _two_minutes = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        };

        private static MemoryCacheEntryOptions _one_day = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
        };

        private static MemoryCacheEntryOptions _one_hour = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };

        private MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly ILogger<GenericCache<TItem>> _logger;

        public GenericCache(ILogger<GenericCache<TItem>> logger)
        {
            _logger = logger;
        }

        public TItem GetOrCreate(CacheType cacheType, string key, Func<TItem> func)
        {
            TItem cacheEntry;
            if (!_cache.TryGetValue(key, out cacheEntry))
            {
                cacheEntry = func();
                _cache.Set(key, cacheEntry, GetCacheOptions(cacheType));
                _logger.LogDebug("Cached key: {key}, {@Entity}", key, JsonSerializer.Serialize(cacheEntry));
            }


            return cacheEntry;
        }

        public async Task<TItem> GetOrCreate(CacheType cacheType, string key, Func<Task<TItem>> func)
        {
            TItem cacheEntry;
            if (!_cache.TryGetValue(key, out cacheEntry))
            {
                cacheEntry = await func();
                _cache.Set(key, cacheEntry, GetCacheOptions(cacheType));
                _logger.LogDebug("Cached key: {key}, {@Entity}", key, JsonSerializer.Serialize(cacheEntry));
            }


            return cacheEntry;
        }

        public async Task<IEnumerable<TItem>> GetOrCreate(CacheType cacheType, string key, Func<Task<IEnumerable<TItem>>> func)
        {
            IEnumerable<TItem> cacheEntry;
            if (!_cache.TryGetValue(key, out cacheEntry))
            {
                cacheEntry = await func();
                _cache.Set(key, cacheEntry, GetCacheOptions(cacheType));
                _logger.LogDebug("Cached key: {key}, {@Entity}", key, JsonSerializer.Serialize(cacheEntry));
            }


            return cacheEntry;
        }

        public IEnumerable<TItem> GetOrCreate(CacheType cacheType, string key, Func<IEnumerable<TItem>> func)
        {
            IEnumerable<TItem> cacheEntry;
            if (!_cache.TryGetValue(key, out cacheEntry))
            {
                cacheEntry = func();
                _cache.Set(key, cacheEntry, GetCacheOptions(cacheType));
                _logger.LogDebug("Cached key: {key}, {@Entity}", key, JsonSerializer.Serialize(cacheEntry));
            }


            return cacheEntry;
        }

        private MemoryCacheEntryOptions GetCacheOptions(CacheType cacheType)
        {
            return cacheType switch
            {
                CacheType.Permission => _two_minutes,
                CacheType.Utility => _one_day,
                CacheType.CompanyLogo => _one_day,
                CacheType.ReportTemplates => _one_day,
                CacheType.Common => _one_hour,
                _ => _two_minutes,
            };
        }
    }

    public enum CacheType
    {
        /// <summary>
        /// 2 minutes cache
        /// </summary>
        Permission,

        /// <summary>
        /// 1 day cache
        /// </summary>
        Utility,

        /// <summary>
        /// 1 hour cache
        /// </summary>
        Common,

        /// <summary>
        /// 1 day cache
        /// </summary>
        CompanyLogo,

        /// <summary>
        /// 1 day cache
        /// </summary>
        ReportTemplates,
    }
}
