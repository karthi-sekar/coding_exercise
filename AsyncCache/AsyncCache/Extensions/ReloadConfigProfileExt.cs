using System;
using System.Collections.Generic;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;

namespace AsyncCache.Extensions
{
    /// <summary>
    /// Ext methods for ReloadConfigProfile;
    /// </summary>
    internal static class ReloadConfigProfileExt
    {
        /// <summary>
        /// Validates config values.
        /// </summary>
        /// <param name="configList">List of profiles to be validated.</param>
        /// <exception cref="AsyncCacheException"></exception>
        public static IDictionary<string, IReloadConfigProfile> Validate(this IList<IReloadConfigProfile> configList)
        {
            try
            {
                configList.ThrowIfNullOrEmpty($"{nameof(IReloadConfigProfile)} list");
                var results = new Dictionary<string, IReloadConfigProfile>();
                foreach (var config in configList)
                {
                    config.Key.ThrowIfNullOrWhiteSpace($"{nameof(IReloadConfigProfile)}.{nameof(config.Key)}");
                    var lowerCaseKey = config.Key.SafeTrimLower();
                    results.ThrowIfHasKey(lowerCaseKey, $"{nameof(IReloadConfigProfile)}");
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

        private static void Validate(this IReloadConfigProfile config)
        {
            switch (config.Type)
            {
                case ReloadType.AtGivenTime:
                    config.Hour.ThrowWhenNotInRange(0, 23,
                        $"For key:{config.Key},{nameof(IReloadConfigProfile)}.{nameof(config.Hour)}");

                    config.Minute.ThrowWhenNotInRange(0, 59,
                        $"For key:{config.Key},{nameof(IReloadConfigProfile)}.{nameof(config.Minute)}");
                    break;
                case ReloadType.Interval:
                    config.Hour.ThrowWhenNotInRange(0, 167,
                        $"For key:{config.Key},{nameof(IReloadConfigProfile)}.{nameof(config.Hour)}");

                    config.Minute.ThrowWhenNotInRange(0, 59,
                        $"For key:{config.Key},{nameof(IReloadConfigProfile)}.{nameof(config.Minute)}");
                    if (config.Hour == 0 && config.Minute < 10)
                    {
                        throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                            $"{nameof(ReloadType)}: Hour/Minute together should not be less than 10 minutes.");
                    }
                    break;
                default:
                    throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                        $"{nameof(ReloadType)}: {config.Type} is not defined.");
            }
        }

        internal static int ReloadDelay(this IReloadConfigProfile profile)
        {
            if (profile == null)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig, 
                    $"{nameof(IReloadConfigProfile)} is null");
            }
            try
            {
                profile.Validate();
                switch (profile.Type)
                {
                    case ReloadType.AtGivenTime:
                        //Once a day
                        return (int) profile.Hour.DifferenceInMsForNextTimestampFromNow(profile.Minute);
                    case ReloadType.Interval:
                        return profile.Hour * 60 * 60 * 1000 + profile.Minute * 60 * 1000;
                    default:
                        throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                            $"{nameof(ReloadType)}: {profile.Type} is not defined.");
                }
            }
            catch (Exception e)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig, "Some config is invalid", e);
            }
        }
    }
}