using System;
using System.Collections.Generic;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;

namespace AsyncCache.Extensions
{
    /// <summary>
    /// Ext methods for cache config.
    /// </summary>
    internal static class CacheConfigProfileExt
    {
        /// <summary>
        /// Validates cache config.
        /// </summary>
        /// <param name="configList">collection of cache configs.</param>
        /// <exception cref="AsyncCacheException"></exception>
        public static IDictionary<Type, ICacheConfigProfile> Validate(this IList<ICacheConfigProfile> configList)
        {
            try
            {
                configList.ThrowIfNullOrEmpty($"{nameof(ICacheConfigProfile)} list");
                var results = new Dictionary<Type, ICacheConfigProfile>();
                foreach (var config in configList)
                {
                    var concreteType = config.ConcreteType;
                    concreteType.ThrowIfNullOrWhiteSpace($"{nameof(ICacheConfigProfile)}.{nameof(config.ConcreteType)}");
                    var cacheType = Type.GetType(concreteType);
                    cacheType.ThrowIfNull($"Type init failed for {nameof(config.ConcreteType)}" +
                        $"(value:{concreteType}).");
                    results.ThrowIfHasKey(cacheType, $"{nameof(ICacheConfigProfile)}");
                    config.Validate(cacheType.Name);
                    results.Add(cacheType, config);
                }
                return results;
            }
            catch (Exception e)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig, "Some config is invalid", e);
            }
        }

        private static void Validate(this ICacheConfigProfile config, string typeName)
        {
            if (config.AdType == AdditionalData.Database)
            {
                config.AdDbProfileKey.ThrowIfNullOrWhiteSpace($"For cache:{typeName}," +
                    $"{nameof(ICacheConfigProfile)}.{nameof(config.AdDbProfileKey)}");
            }
            config.DbProfileKey.ThrowIfNullOrWhiteSpace($"For cache:{typeName}," +
                $"{nameof(ICacheConfigProfile)}.{nameof(config.DbProfileKey)}");

            config.PartitionCount.ThrowIfNegative($"For cache:{typeName}," +
                $"{nameof(ICacheConfigProfile)}.{nameof(config.PartitionCount)}");

            config.ReloadProfileKey.ThrowIfNullOrWhiteSpace($"For cache:{typeName}," +
                $"{nameof(ICacheConfigProfile)}.{nameof(config.ReloadProfileKey)}");

            config.SerializationProfileKey.ThrowIfNullOrWhiteSpace($"For cache:{typeName}," +
                $"{nameof(ICacheConfigProfile)}.{nameof(config.SerializationProfileKey)}");
        }
    }
}