using System;
using System.Collections.Generic;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;

namespace AsyncCache.Extensions
{
    /// <summary>
    /// Db profile config defs.
    /// </summary>
    internal static class DbConfigProfileExt
    {
        /// <summary>
        /// Validates config.
        /// </summary>
        /// <param name="configList">list of profiles.</param>
        /// <exception cref="AsyncCacheException"></exception>
        public static IDictionary<string, IDbConfigProfile> Validate(this IList<IDbConfigProfile> configList)
        {
            try
            {
                configList.ThrowIfNullOrEmpty($"{nameof(IDbConfigProfile)} list");
                var results = new Dictionary<string, IDbConfigProfile>();
                foreach (var config in configList)
                {
                    config.Key.ThrowIfNullOrWhiteSpace($"{nameof(IDbConfigProfile)}.{nameof(config.Key)}");
                    var lowerCaseKey = config.Key.SafeTrimLower();
                    results.ThrowIfHasKey(lowerCaseKey, $"{nameof(IDbConfigProfile)}");
                    config.Validate();
                    results.Add(lowerCaseKey, config);
                }
                return results;
            }
            catch (Exception e)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig, "Some config is invalid", e);
            }
        }

        private static void Validate(this IDbConfigProfile config)
        {
            config.ConnectionType.ThrowIfNullOrWhiteSpace($"For key:{config.Key}," +
                $"{nameof(IDbConfigProfile)}.{nameof(config.ConnectionType)}");

            config.ConnectionString.ThrowIfNullOrWhiteSpace($"For key:{config.Key}," +
                $"{nameof(IDbConfigProfile)}.{nameof(config.ConnectionString)}");
        }
    }
}