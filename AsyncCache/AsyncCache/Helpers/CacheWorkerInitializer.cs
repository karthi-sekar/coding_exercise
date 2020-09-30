using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Lib;
using AsyncCache.Lib.CacheDataWorkerImpl;
using AsyncCache.Lib.CacheSerializerImpl;
using AsyncCache.Lib.DbRelated;
using AsyncCacheContract;
using AsyncCacheContract.Cache;
using AsyncCacheContract.Enums;

namespace AsyncCache.Helpers
{
    internal sealed class CacheWorkerInitializer<TMap, TKey, TValue> where TMap : DbMap<TKey, TValue>, new()
    {
        private readonly InitInput _config;
        private readonly string _actualQuery;
        private readonly Func<TKey, string> _keyBasedQueryFunc;
        private readonly Func<string> _additionalQueryFunc;
        private readonly string _cacheName;
        private readonly CancellationToken _token;
        private readonly ObjectPool<TMap> _objPool = new ObjectPool<TMap>();
        private CacheDataWorker<TKey, TValue> _cacheDataWorker;
        private CacheDataWorker<TKey, TValue> _additionalDataWorker;

        public CacheWorkerInitializer(InitInput config, string actualQuery, Func<TKey, string> keyBasedQueryFunc,
            Func<string> additionalQueryFunc, string cacheName, CancellationToken token)
        {
            _config = config;
            _actualQuery = actualQuery;
            _keyBasedQueryFunc = keyBasedQueryFunc;
            _additionalQueryFunc = additionalQueryFunc;
            _cacheName = cacheName;
            _token = token;
        }

        /// <summary>
        /// First item is actual query data worker and second is for additional data.
        /// <para>IMPORTANT: To be used ONLY for Init operation.</para>
        /// </summary>
        public async Task<Tuple<CacheDataWorker<TKey, TValue>, CacheDataWorker<TKey, TValue>>> InitInstances()
        {
            var actualDataWorker = await CreateDataWorker(true).ConfigureAwait(false);
            var additionalDataWorker = await CreateDataWorker(false).ConfigureAwait(false);
            return new Tuple<CacheDataWorker<TKey, TValue>, CacheDataWorker<TKey, TValue>>(actualDataWorker,
                additionalDataWorker);
        }

        /// <summary>
        /// First item is actual query data loader and second is for additional data.
        /// <para>IMPORTANT: To be used ONLY after Init operation.</para>
        /// </summary>
        public Tuple<CacheDataWorker<TKey, TValue>, CacheDataWorker<TKey, TValue>> PostInitInstances()
        {
            _cacheDataWorker.Fallback = null;
            _additionalDataWorker.Fallback = null;
            return new Tuple<CacheDataWorker<TKey, TValue>, CacheDataWorker<TKey, TValue>>(_cacheDataWorker,
                _additionalDataWorker);
        }

        private async Task<CacheDataWorker<TKey, TValue>> CreateDataWorker(bool forActualQuery)
        {
            if (forActualQuery)
            {
                if (_config.CacheConfig.ReloadFromFileFirst)
                {
                    var fileLoader = FileDataLoader();
                    _cacheDataWorker = await DbDataLoader(true).ConfigureAwait(false);
                    fileLoader.Fallback = _cacheDataWorker;
                    return fileLoader;
                }
                var dbloader = await DbDataLoader(true).ConfigureAwait(false);
                dbloader.Fallback = FileDataLoader();
                _cacheDataWorker = dbloader;
                return dbloader;
            }
            if (_config.CacheConfig.AdType != AdditionalData.Database)
            {
                _additionalDataWorker = FileDataLoader();
                return _additionalDataWorker;
            }
            if (_config.CacheConfig.ReloadFromFileFirst)
            {
                var adfileLoader = FileDataLoader();
                _additionalDataWorker = await DbDataLoader(false).ConfigureAwait(false);
                adfileLoader.Fallback = _additionalDataWorker;
                return adfileLoader;
            }
            var addbloader = await DbDataLoader(false).ConfigureAwait(false);
            addbloader.Fallback = FileDataLoader();
            _additionalDataWorker = addbloader;
            return addbloader;
        }

        private CacheDataWorker<TKey, TValue> FileDataLoader()
        {
            return new FileDataWorker<TKey, TValue>(_cacheName, _config.Logger);
        }

        private async Task<CacheDataWorker<TKey, TValue>> DbDataLoader(bool forActualQuery)
        {
            if (forActualQuery)
            {
                var dbpropMapper = new DatabasePropertyMapper<TMap>(_config.ActualDataConnectionMgr, _cacheName, _token,
                    _actualQuery, true);
                await dbpropMapper.PrepareBinding().ConfigureAwait(false);
                var asyncDbFetcher = new AsyncDbDataFetcher<TMap, TKey, TValue>(_config.ActualDataConnectionMgr,
                    _cacheName, _actualQuery, dbpropMapper.QuerySetters, _objPool, _keyBasedQueryFunc, _config.Logger);
                return new DbDataWorker<TMap, TKey, TValue>(asyncDbFetcher, asyncDbFetcher, CreateCacheSerializer(true),
                    _objPool);
            }
            else
            {
                var dbpropMapper = new DatabasePropertyMapper<TMap>(_config.AdditionalDataConnectionMgr,
                    _cacheName + "-ad", _token, _additionalQueryFunc(), false);
                await dbpropMapper.PrepareBinding().ConfigureAwait(false);
                var asyncDbFetcher = new AsyncDbDataFetcher<TMap, TKey, TValue>(_config.AdditionalDataConnectionMgr,
                    _cacheName + "-ad", _additionalQueryFunc(), dbpropMapper.QuerySetters, _objPool, null, _config.Logger);
                return new DbDataWorker<TMap, TKey, TValue>(asyncDbFetcher, asyncDbFetcher, CreateCacheSerializer(false),
                    _objPool);
            }
        }

        private CacheSerializer CreateCacheSerializer(bool forActualQuery)
        {
            switch (_config.SerializationConfig.Type)
            {
                case SerializationType.AtGivenTime:
                    return new ScheduledSerializer(_config.SerializationConfig, _config.Logger,
                        forActualQuery ? _cacheName : _cacheName + "-ad");
                case SerializationType.EveryReload:
                    return new OnReloadSerializer();
                case SerializationType.Never:
                    return new NeverSerializer();
                case SerializationType.AtShutdown:
                    return new ShutdownSerializer();
                default:
                    throw new AsyncCacheException(AsyncCacheErrorCode.InvalidImplementation,
                        $"({_cacheName}) Serialization type not known:{_config.SerializationConfig.Type}");
            }
        }
    }
}