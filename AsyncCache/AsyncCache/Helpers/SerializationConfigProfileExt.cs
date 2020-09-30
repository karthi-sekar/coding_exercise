using System;
using System.Collections.Generic;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;

namespace AsyncCache.Extensions
{
    /// <summary>
    /// Ext methods for SerializationConfigProfile.
    /// </summary>
    internal static class SerializationConfigProfileExt
    {
        /// <summary>
        /// Validates config.
        /// </summary>
        /// <param name="configList">list of profiles to validate.</param>
        /// <exception cref="AsyncCacheException"></exception>
        public static IDictionary<string, ISerializationConfigProfile> Validate(
            this IList<ISerializationConfigProfile> configList)
        {
            try
            {
                configList.ThrowIfNullOrEmpty($"{nameof(ISerializationConfigProfile)} list");
                var results = new Dictionary<string, ISerializationConfigProfile>();
                foreach (var config in configList)
                {
                    config.Key.ThrowIfNullOrWhiteSpace($"{nameof(ISerializationConfigProfile)}.{nameof(config.Key)}");
                    var lowerCaseKey = config.Key.SafeTrimLower();
                    results.ThrowIfHasKey(lowerCaseKey, $"{nameof(ISerializationConfigProfile)}");
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

        private static void Validate(this ISerializationConfigProfile config)
        {
            if (config.Type != SerializationType.AtGivenTime)
            {
                return;
            }
            config.Hour.ThrowWhenNotInRange(0, 23,
                $"For key:{config.Key},{nameof(ISerializationConfigProfile)}.{nameof(config.Hour)}");

            config.Minute.ThrowWhenNotInRange(0, 59,
                $"For key:{config.Key},{nameof(ISerializationConfigProfile)}.{nameof(config.Minute)}");
        }
    }
}