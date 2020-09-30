using System;
using System.IO;
using System.Threading;
using AsyncCache.Contracts;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;
using log4net;

namespace AsyncCache.Helpers
{
    /// <summary>
    /// Input data for AsyncCache during Init call.
    /// </summary>
    internal sealed class InitInput
    {
        public ICacheConfigProfile CacheConfig { get; set; }
        public IReloadConfigProfile ReloadConfig { get; set; }
        public ISerializationConfigProfile SerializationConfig { get; set; }
        public DirectoryInfo TopLevelLocalDirectory { get; set; }
        public IConnectionMgr ActualDataConnectionMgr { get; set; }
        public IConnectionMgr AdditionalDataConnectionMgr { get; set; }
        public ILog Logger { get; set; }
        public CancellationToken Token { get; set; }

        public InitInput Validate()
        {
            try
            {
                CacheConfig.ThrowIfNull($"{nameof(CacheConfig)}");
                if (CacheConfig.AdType == AdditionalData.Database)
                {
                    AdditionalDataConnectionMgr.ThrowIfNull($"{nameof(AdditionalDataConnectionMgr)}");
                }
                ReloadConfig.ThrowIfNull($"{nameof(ReloadConfig)}");
                SerializationConfig.ThrowIfNull($"{nameof(SerializationConfig)}");
                TopLevelLocalDirectory.ThrowIfNullOrNonExisting($"{nameof(TopLevelLocalDirectory)}");
                ActualDataConnectionMgr.ThrowIfNull($"{nameof(ActualDataConnectionMgr)}");
                Logger.ThrowIfNull($"{nameof(Logger)}");

                return this;
            }
            catch (ArgumentException e)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig, "Some config is invalid", e);
            }
        }
    }
}