using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using AsyncCache.Extensions;
using AsyncCache.Helpers;
using AsyncCacheContract;
using AsyncCacheContract.Cache;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;
using log4net;

namespace AsyncCache
{
    /// <summary>
    /// Async cache manager class.
    /// <para>Provides means to manage all cache through config.</para>
    /// </summary>
    public sealed class AsyncCacheMgr : ICacheMgr
    {
        private readonly IDictionary<Type, ICacheConfigProfile> _cacheProfiles;
        private readonly ConcurrentDictionary<Type, object> _cacheDictionary;
        private readonly ConcurrentQueue<IAsyncCache> _cacheCollection;
        private readonly InitInputBuilder _initInputBuilder;
        private readonly int _maxConcurrency;
        private readonly object _syncRoot = new object();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Default Cotr.
        /// </summary>
        /// <param name="config">Cache config</param>
        /// <param name="logger">Logger</param>
        /// <param name="connectionGenerator">Accepts ConnectionType (first argument) and 
        /// ConnectionString (second argument) as input and returns an instance of DB connection (Must NOT be Opened).
        /// <para>Both ConnectionType and ConnectionString are exactly what mentioned in DbProfile Config.</para></param>
        public AsyncCacheMgr(IAsyncCacheConfig config, ILog logger,
            Func<string, string, DbConnection> connectionGenerator)
        {
            config.ThrowIfNull($"{nameof(AsyncCacheMgr)}.{nameof(config)}");
            logger.ThrowIfNull($"{nameof(AsyncCacheMgr)}.{nameof(logger)}");
            connectionGenerator.ThrowIfNull($"{nameof(AsyncCacheMgr)}.{nameof(connectionGenerator)}");

            config.Validate();

            _maxConcurrency = StaticCalls.AdjustMaxConcurrency(config.MaxConcurrency);
            var connectionBlocker = new BlockingCollection<string>(_maxConcurrency);
            for (var i = 0; i < _maxConcurrency; i++)
            {
                connectionBlocker.Add("DB" + i);
            }

            _cacheProfiles = config.CacheProfiles.Validate();
            _initInputBuilder = new InitInputBuilder
            {
                DatabaseProfiles = config.DbProfiles.Validate(),
                ReloadProfiles = config.ReloadProfiles.Validate(),
                SerializationProfiles = config.SerializationProfiles.Validate(),
                LocalCacheFolder = config.CacheLocalFolder.ToDirectoryInfo(),
                ConnectionBlocker = connectionBlocker,
                ConnectionGenerator = connectionGenerator,
                Logger = logger,
                Token = _cts.Token
            };

            _cacheCollection = new ConcurrentQueue<IAsyncCache>();
            _cacheDictionary = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        /// Initializes/schedules all caches.
        /// </summary>
        public void Init()
        {
            var keyArray = _cacheProfiles.Keys.ToArray();
            lock (_syncRoot)
            {
                Parallel.For(0, keyArray.Length, new ParallelOptions
                {
                    MaxDegreeOfParallelism = _maxConcurrency,
                    CancellationToken = _cts.Token
                }, i =>
                {
                    var key = keyArray[i];
                    var cacheProfile = _cacheProfiles[key];
                    if (cacheProfile.Active)
                    {
                        var cacheInstance = Activator.CreateInstance(key) as IAsyncCache;
                        if (cacheInstance == null)
                        {
                            throw new AsyncCacheException(AsyncCacheErrorCode.InvalidCache,
                                $"Default Ctor missing for AsyncCache (type:{cacheProfile.ConcreteType})" +
                                    "or cache instance is NOT derived from AsyncCache<TKey,TValue>.");
                        }

                        _cacheDictionary.TryAdd(key, cacheInstance);
                        _cacheCollection.Enqueue(cacheInstance);
                        //Create init tasks
                        cacheInstance.InitAsync(_initInputBuilder.Prepare(cacheProfile)).Wait(_cts.Token);
                    }
                    else
                    {
                        _cacheDictionary.TryAdd(key, null);
                    }
                });
            }
        }

        /// <summary>
        /// Stops/Disposes all caches.
        /// </summary>
        public void Dispose()
        {
            _cts.Cancel();
            using (_cts)
            {
                _cacheDictionary.Clear();
                lock (_syncRoot)
                {
                    Parallel.For(0, _cacheCollection.Count, i =>
                    {
                        IAsyncCache cache;
                        if (_cacheCollection.TryDequeue(out cache))
                        {
                            cache?.Dispose().Wait(CancellationToken.None);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Gets all initialized instances of caches (as defined in config).
        /// <para>Dic6tionary Key is the type as mentioned in Config and value is the instance.</para>
        /// <para>If some cache were Inactive (Active = false in config), then value is null</para>
        /// </summary>
        public IDictionary<Type, object> Caches => new Dictionary<Type, object>(_cacheDictionary);
    }
}