using Core.Cache.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Cache.Implementation
{
    public class CacheProvider : ICacheProvider
    {
        private static readonly SemaphoreSlim Locker = new(1, 1);
        private readonly IMemoryCache _cache;

        public CacheProvider(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<string> GetOrCreateAsync(object key, string value = null)
        {
            await Locker.WaitAsync();

            try
            {
                if (_cache.TryGetValue(key, out string valueCache))
                {
                    return valueCache;
                }
                else
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        return _cache.Set(key, value, new MemoryCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                        });
                    }
                }
            }
            finally
            {
                Locker.Release();
            }

            return string.Empty;
        }
    }
}