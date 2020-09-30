using System;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;

namespace AsyncCache.Extensions
{
    /// <summary>
    /// Ext methods for Cache config.
    /// </summary>
    internal static class AsyncCacheConfigExt
    {
        /// <summary>
        /// Basic config validation.
        /// </summary>
        /// <param name="config">instance value to validate.</param>
        public static void Validate(this IAsyncCacheConfig config)
        {
            try
            {
                config.ThrowIfNull(nameof(IAsyncCacheConfig));
                config.CacheLocalFolder.ThrowIfNullOrWhiteSpace($"{nameof(IAsyncCacheConfig)}" +
                    $".{nameof(config.CacheLocalFolder)}");
                config.MaxConcurrency.ThrowIfNegative($"{nameof(IAsyncCacheConfig)}.{nameof(config.MaxConcurrency)}");
                config.CacheProfiles.ThrowIfNullOrEmpty($"{nameof(IAsyncCacheConfig)}.{nameof(config.CacheProfiles)}");
                config.ReloadProfiles.ThrowIfNullOrEmpty($"{nameof(IAsyncCacheConfig)}.{nameof(config.ReloadProfiles)}");
                config.DbProfiles.ThrowIfNullOrEmpty($"{nameof(IAsyncCacheConfig)}.{nameof(config.DbProfiles)}");
                config.SerializationProfiles.ThrowIfNullOrEmpty($"{nameof(IAsyncCacheConfig)}" +
                    $".{nameof(config.SerializationProfiles)}");
            }
            catch (Exception e)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig, "Some config is invalid", e);
            }
        }
    }
}