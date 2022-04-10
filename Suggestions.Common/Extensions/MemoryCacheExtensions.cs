using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Contrib.DuplicateRequestCollapser;

namespace Suggestions.Common.Extensions
{
    public static class MemoryCacheExtensions
    {
        // Предотвращает параллельное обновление кэша для одного и того же ключа
        private static readonly IAsyncRequestCollapserPolicy collapserPolicy;

        static MemoryCacheExtensions()
        {
            collapserPolicy = AsyncRequestCollapserPolicy.Create();
        }

        public static async Task<T> Get<T>(this IMemoryCache cache, string key, Func<Task<T>> funcGetter, TimeSpan ttl)
            where T : class
        {
            var data = cache.Get<T>(key);
            if (data != null)
            {
                return data;
            }

            var item = await collapserPolicy.ExecuteAsync(async ctx => await funcGetter(), new Context(key));
            cache.Set(key, item, ttl);
            return item;
        }
    }
}