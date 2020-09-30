using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using AsyncCache.Lib.DbRelated;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;
using log4net;

// ReSharper disable MemberCanBePrivate.Global

namespace AsyncCache.Helpers
{
    internal sealed class InitInputBuilder
    {
        public IDictionary<string, IDbConfigProfile> DatabaseProfiles { get; set; }
        public IDictionary<string, IReloadConfigProfile> ReloadProfiles { get; set; }
        public IDictionary<string, ISerializationConfigProfile> SerializationProfiles { get; set; }
        public DirectoryInfo LocalCacheFolder { get; set; }
        public BlockingCollection<string> ConnectionBlocker { get; set; }
        public Func<string, string, DbConnection> ConnectionGenerator { get; set; }
        public ILog Logger { get; set; }
        public CancellationToken Token { get; set; }

        public InitInput Prepare(ICacheConfigProfile profile)
        {
            var result = new InitInput
            {
                CacheConfig = profile,
                TopLevelLocalDirectory = LocalCacheFolder,
                Logger = Logger,
                Token = Token
            };
            IDbConfigProfile dbprofile;
            if (profile.AdType == AdditionalData.Database)
            {
                if (DatabaseProfiles.TryGetValue(profile.AdDbProfileKey.SafeTrimLower(), out dbprofile))
                {
                    result.AdditionalDataConnectionMgr = new DbConnectionMgr(ConnectionBlocker, ConnectionGenerator,
                        dbprofile.ConnectionType, dbprofile.ConnectionString, Logger);
                }
                else
                {
                    throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                        $"For cache (Type:{profile.ConcreteType}), unable to find " +
                            $"Additional data db profile (key:{profile.AdDbProfileKey})");
                }
            }
            else
            {
                result.AdditionalDataConnectionMgr = null;
            }
            if (DatabaseProfiles.TryGetValue(profile.DbProfileKey.SafeTrimLower(), out dbprofile))
            {
                result.ActualDataConnectionMgr = new DbConnectionMgr(ConnectionBlocker, ConnectionGenerator,
                    dbprofile.ConnectionType, dbprofile.ConnectionString, Logger);
            }
            else
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                    $"For cache (Type:{profile.ConcreteType}), unable to find " +
                        $"actual data db profile (key:{profile.DbProfileKey})");
            }
            IReloadConfigProfile reloadConfig;
            if (ReloadProfiles.TryGetValue(profile.ReloadProfileKey.SafeTrimLower(), out reloadConfig))
            {
                result.ReloadConfig = reloadConfig;
            }
            else
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                    $"For cache (Type:{profile.ConcreteType}), unable to find " +
                        $"reload config profile (key:{profile.ReloadProfileKey})");
            }
            ISerializationConfigProfile serialConfig;
            if (SerializationProfiles.TryGetValue(profile.SerializationProfileKey.SafeTrimLower(), out serialConfig))
            {
                if (profile.ReloadFromFileFirst && serialConfig.Type == SerializationType.Never)
                {
                    throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                        $"For cache (Type:{profile.ConcreteType}), {nameof(profile.ReloadFromFileFirst)}" +
                            "=true but serialization config profile has " +
                            $"{nameof(serialConfig.Type)}={SerializationType.Never}");
                }
                result.SerializationConfig = serialConfig;
            }
            else
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                    $"For cache (Type:{profile.ConcreteType}), unable to find " +
                        $"serialization config profile (key:{profile.SerializationProfileKey})");
            }

            return result;
        }
    }
}