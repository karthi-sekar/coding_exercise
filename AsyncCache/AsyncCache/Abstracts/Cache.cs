using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using AsyncCache.Helpers;
using AsyncCache.Lib;
using AsyncCache.Lib.CacheDataImpl;
using AsyncCache.Lib.DataSerializers;
using AsyncCacheContract;
using AsyncCacheContract.Cache;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using log4net;

namespace AsyncCache.Abstracts
{
    /// <summary>
    /// Default Base class for AsyncCache.
    /// </summary>
    /// <typeparam name="TMap">Intermediate class which will generate Key and Value instances.
    /// All the Db values will be mapped on this class.</typeparam>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    public abstract class Cache<TMap, TKey, TValue> : Cache<TKey, TValue>, IAlterableCache<TKey, TValue>, IAsyncCache, IReloadable
        where TMap : DbMap<TKey, TValue>, new()
    {
        private const string AdSubFolder = "Ad";
        private readonly CancellationTokenSource _cts;
        private readonly AutoResetEvent _syncRoot;
        private readonly string _name;
        private readonly bool _xmlSerialization;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly IEnumerable<Type> _dataContractKnownType;
        private ILog _logger;
        private AsyncCacheData<TKey, TValue> _cacheData;
        private AsyncCacheData<TKey, TValue> _additionalData;
        private CacheDataWorker<TKey, TValue> _cacheWorker;
        private CacheDataWorker<TKey, TValue> _additionalDataWorker;
        private CacheScheduler _cacheScheduler;
        private bool _gotoDbForMissingKey;
        private CancellationTokenSource _combinedCts;
        private CancellationToken _cancelToken;
        private volatile bool _runningInInitPhase;

        #region Ctors

        private Cache()
        {
            _cts = new CancellationTokenSource();
            _cancelToken = _cts.Token;
            //Do NOT change this naming convention, many thinks may fall.
            _name = GetType().Name;
            _syncRoot = new AutoResetEvent(true);
            _runningInInitPhase = true;
        }

        /// <summary>
        /// Default ctor for Cache which uses XML serialization when serializing cache to file.
        /// </summary>
        /// <param name="keyComparer">key comparer</param>
        /// <param name="usexmlSerializer">True to use XML Serialization and false for JSON.</param>
        /// <param name="dataContractKnownType">Known type for XML/JSON based data contract serializer.
        /// <para>If there is no known type, provide null</para></param>
        protected Cache(IEqualityComparer<TKey> keyComparer, bool usexmlSerializer, IEnumerable<Type> dataContractKnownType) : this()
        {
            _keyComparer = keyComparer;
            _dataContractKnownType = dataContractKnownType;
            _xmlSerialization = usexmlSerializer;
        }

        #endregion

        /// <summary>
        /// Returns true with AsyncCacheValue instance when key is in cache. Else returns false and default value.
        /// <para>Do NOT call this method and then check for out value for nullness. Good code should make 
        /// decisions based on bool output.</para>
        /// <para>When AsyncCache config has KeyIsValue = True (because KEY itself is value), out value is ALWAYS equal to key.
        /// Thus make the decision based on boolean output.</para>
        /// <para>KeyIsValue = False, and output is True, then out value is NOT Null.</para>
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="value">Out value. When AsyncCache config has KeyIsValue = True (because KEY itself is value),
        /// out value is ALWAYS equal to key. Thus make the decision based on boolean output.</param>
        public sealed override bool TryFind(TKey key, out TValue value)
        {
            if (Volatile.Read(ref _cacheData).TryGet(key, out value))
            {
                return true;
            }
            try
            {
                return (_gotoDbForMissingKey || _runningInInitPhase) && _cacheWorker != null &&
                    _cacheWorker.AddOrReplaceForAsync(key, _cacheData, _cancelToken).Result &&
                    Volatile.Read(ref _cacheData).TryGet(key, out value);
            }
            catch (OperationCanceledException)
            {
                _logger.Error($"(MINOR)({_name}) TryFind error due to cancellation");
                return false;
            }
            catch (AsyncCacheException e)
            {
                _logger.ErrorFormat($"(MAJOR)({_name}) TryFind error", e);
                return false;
            }
        }

        #region IAlterableCache related.

        /// <summary>
        /// Tries to remove the given key.
        /// </summary>
        /// <param name="key">Key to remove</param>
        public bool TryRemove(TKey key)
        {
            return Volatile.Read(ref _cacheData).TryRemove(key);
        }

        /// <summary>
        /// Adds or replace the value associated to the key.
        /// </summary>
        /// <param name="key">Key instance</param>
        /// <param name="value">Value instance</param>
        public void AddOrReplace(TKey key, TValue value)
        {
            Volatile.Read(ref _cacheData).AddOrReplace(key, value);
        }

        /// <summary>
        /// Based on the key, tries to refresh data from db.
        /// <para>If true is returned, means data was found in DB else false is returned.</para>
        /// </summary>
        /// <param name="key">Key to refresh.</param>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="AsyncCacheException"></exception>
        public async Task<bool> RefreshFromDb(TKey key)
        {
            if (_cacheWorker != null)
            {
                return await _cacheWorker.AddOrReplaceForAsync(key, _cacheData, _cancelToken).ConfigureAwait(false);
            }
            return false;
        }

        /// <summary>
        /// Forced full reload.
        /// </summary>
        public async Task RefreshFromDb(CancellationToken token)
        {
            var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancelToken, token);
            using (combinedCts)
            {
                await ((IReloadable) this).Reload(combinedCts.Token).ConfigureAwait(false);
            }
        }

        #endregion

        #region Abstract Part

        /// <summary>
        /// Query of the Actual Data.
        /// <para>Uses AsyncCacheAttribute to map Db column to properties.</para>
        /// </summary>
        protected abstract string CacheQuery { get; }

        /// <summary>
        /// When Cache config has AdType = AdditionalData.Database. This property must return the query
        /// to fetch the additional data from DB.
        /// <para>Uses AsyncAdCacheAttribute to map Db column to properties.</para>
        /// </summary>
        protected abstract string AdditionalDataQuery { get; }

        /// <summary>
        /// This method will be called when key is missing and DB need to be checked to retrieve data 
        /// (i.e. when config has GoToDbForMissingKey = true).
        /// <para>IMPORTANT: It has NOTHING to do with additional data.</para>
        /// </summary>
        /// <param name="key">missing key instance.</param>
        protected abstract string CacheQueryForMissingKey(TKey key);

        #endregion

        #region IAsyncCache implementation

        async Task IAsyncCache.InitAsync(InitInput input)
        {
            var entered = false;
            try
            {
                entered = _syncRoot.WaitOne(Timeout.Infinite);
                if (entered)
                {
                    input.Validate();
                    _logger = input.Logger;
                    _combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, input.Token);
                    _cancelToken = _combinedCts.Token;

                    _gotoDbForMissingKey = input.CacheConfig.GoToDbForMissingKey;
                    ((IReloadable)this).ReloadOnceImmediate = input.CacheConfig.ReloadFromFileFirst;

                    _cacheData = CreateCacheDataInstance(input.CacheConfig,
                        CreateCacheSerializer(input.TopLevelLocalDirectory, false));
                    _additionalData = CreateAdditionalCacheDataInstance(input.CacheConfig,
                        CreateCacheSerializer(input.TopLevelLocalDirectory.CreateSubdirectory(AdSubFolder), true));

                    var cacheWorkerInitializer = CacheWorkerInitializer(input);
                    var instances = await cacheWorkerInitializer.InitInstances().ConfigureAwait(false);
                    //item 1 is actual cache worker
                    _cacheWorker = instances.Item1;
                    //item 2 is additional data worker.
                    _additionalDataWorker = instances.Item2;

                    await InternalReload(_cancelToken).ConfigureAwait(false);

                    //Thread.MemoryBarrier();

                    //Post init actions.
                    instances = cacheWorkerInitializer.PostInitInstances();
                    //item 1 is actual cache worker
                    _cacheWorker = instances.Item1;
                    //item 2 is additional data worker.
                    _additionalDataWorker = instances.Item2;

                    //Thread.MemoryBarrier();

                    _cacheScheduler = new CacheScheduler(this, input.ReloadConfig, _cancelToken, _logger, _name);
                    _runningInInitPhase = false;
                }
            }
            finally
            {
                if (entered)
                {
                    _syncRoot.Set();
                }
            }
        }

        async Task IAsyncCache.Dispose()
        {
            _cts.Cancel();
            using (_cts)
            {
                if (_syncRoot.WaitOne(Timeout.Infinite))
                {
                    if (_cacheScheduler != null)
                    {
                        await _cacheScheduler.Dispose().ConfigureAwait(false);
                        _cacheScheduler = null;
                    }
                    if (_cacheWorker != null)
                    {
                        await _cacheWorker.Shutdown().ConfigureAwait(false);
                    }
                    if (_additionalDataWorker != null)
                    {
                        await _additionalDataWorker.Shutdown().ConfigureAwait(false);
                    }
                    _syncRoot.Set();
                }
                if (_combinedCts != null)
                {
                    using (_combinedCts)
                    {
                        //to dispose.
                    }
                }
            }
        }

        #endregion

        #region IReloadable Implementation

        Task IReloadable.Reload(CancellationToken token)
        {
            var entered = false;
            try
            {
                entered = _syncRoot.WaitOne(TimeSpan.Zero);
                if (entered)
                {
                    token.ThrowIfCancellationRequested();
                    return InternalReload(token);
                }
                else
                {
                    return Task.CompletedTask;
                }
            }
            finally
            {
                if (entered)
                {
                    _syncRoot.Set();
                }
            }
        }

        bool IReloadable.ReloadOnceImmediate { get; set; }

        #endregion

        #region Private Methods

        private async Task InternalReload(CancellationToken token)
        {
            //we create new instance of actual cache, so that during reload old cache remains in use.
            var newCache = _cacheData.CreateEmpty();
            //Just start the task (to download/serialize actual cache data) but dont await.
            var actualCacheTask = _cacheWorker.FreshLoadToAsync(newCache, token);
            try
            {
                //Now we will load/serialize additional data if any... this should be fast.
                //we do NOT create new instance of additional data as it would be merged with
                //actual cache from which lookups would be made.
                await _additionalDataWorker.FreshLoadToAsync(_additionalData, token).ConfigureAwait(false);
            }
            finally
            {
                //now we wait for actual data loading
                await actualCacheTask.ConfigureAwait(false);
            }
            //we merge additional data to actual data
            _additionalData.Merge(newCache, token);
            Interlocked.Exchange(ref _cacheData, newCache);
        }

        private ICacheDataSerializer CreateCacheSerializer(DirectoryInfo folder, bool indentData)
        {
            return _xmlSerialization
                ? (ICacheDataSerializer)new XmlSerializer(_name, folder, _dataContractKnownType, indentData)
                : new JsonSerializer(_name, folder, _dataContractKnownType);
        }

        private AsyncCacheData<TKey, TValue> CreateCacheDataInstance(ICacheConfigProfile config,
            ICacheDataSerializer serializer)
        {
            var partitions = StaticCalls.AdjustPartitionCount(config.PartitionCount);
            if (!config.KeyIsValue)
            {
                return new DictionaryCacheData<TKey, TValue>(partitions, _keyComparer, serializer);
            }
            if (typeof(TKey) == typeof(TValue))
            {
                return new HashSetCacheData<TKey>(partitions, _keyComparer, serializer) as AsyncCacheData<TKey, TValue>;
            }
            throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                $"Config sets {nameof(config.KeyIsValue)} = true. But TKey and TValue not same type.");
        }

        private AsyncCacheData<TKey, TValue> CreateAdditionalCacheDataInstance(ICacheConfigProfile config,
            ICacheDataSerializer serializer)
        {
            if (config.AdType == AdditionalData.None)
            {
                return new EmptyCacheData<TKey, TValue>(0, null);
            }
            if (!config.KeyIsValue)
            {
                return new DictionaryCacheData<TKey, TValue>(1, _keyComparer, serializer);
            }
            if (typeof(TKey) == typeof(TValue))
            {
                return new HashSetCacheData<TKey>(1, _keyComparer, serializer) as AsyncCacheData<TKey, TValue>;
            }
            throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                $"Config sets {nameof(config.KeyIsValue)} = true. But TKey and TValue not same type.");
        }

        private CacheWorkerInitializer<TMap, TKey, TValue> CacheWorkerInitializer(InitInput config)
        {
            Expression<Func<TKey, string>> expr = key => CacheQueryForMissingKey(key);
            return new CacheWorkerInitializer<TMap, TKey, TValue>(config, CacheQuery, expr.Compile(),
                () => AdditionalDataQuery, _name, _cancelToken);
        }

        #endregion
    }
}