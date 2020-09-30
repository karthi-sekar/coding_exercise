using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;
using AsyncCacheContract;
using AsyncCacheContract.Cache;
using AsyncCacheContract.Enums;

namespace AsyncCache.Lib.CacheDataWorkerImpl
{
    internal sealed class DbDataWorker<TMap, TKey, TValue> : CacheDataWorker<TKey, TValue>
        where TMap : DbMap<TKey, TValue>, new()
    {
        private readonly CancellationTokenSource _localCts = new CancellationTokenSource();
        private readonly IAsyncDbDataFetcher<TMap> _fetcher;
        private readonly IDbDataFetcher<TMap, TKey, TValue> _keyBasedFetcher;
        private readonly CacheSerializer _cacheSerializer;
        private readonly ObjectPool<TMap> _mapPool;

        public DbDataWorker(IAsyncDbDataFetcher<TMap> fetcher, IDbDataFetcher<TMap, TKey, TValue> keyBasedFetcher,
            CacheSerializer cacheSerializer, ObjectPool<TMap> mapPool)
        {
            _fetcher = fetcher;
            _keyBasedFetcher = keyBasedFetcher;
            _cacheSerializer = cacheSerializer;
            _mapPool = mapPool;
        }

        protected override async Task LoadAsync(AsyncCacheData<TKey, TValue> emptyData, CancellationToken token)
        {
            var operationCts = new CancellationTokenSource();
            var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, operationCts.Token,
                _localCts.Token);
            var combinedToken = combinedTokenSource.Token;

            //Keep this collection with 1 element bound.
            var collection = new BlockingCollection<TMap>(1);
            var cachePopulatorTask = emptyData.ValueTypeCanBeMerged
                ? Task.Factory.StartNew(() => PopulateCacheWithMerging(emptyData, collection, combinedToken, operationCts),
                    combinedToken)
                : Task.Factory.StartNew(
                                        () => PopulateCacheWithoutMerging(emptyData, collection, combinedToken, operationCts),
                    combinedToken);

            try
            {
                await _fetcher.FillAsync(collection, combinedToken).ConfigureAwait(false);
            }
            finally
            {
                collection.CompleteAdding();
                using (operationCts)
                {
                    using (combinedTokenSource)
                    {
                        using (collection)
                        {
                            await cachePopulatorTask.ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        protected override Task SerializeAsync(ISerializableCache data)
        {
            _localCts.Token.ThrowIfCancellationRequested();
            return _cacheSerializer.SerializeAsync(data);
        }

        public override Task<bool> AddOrReplaceForAsync(TKey key, IAsyncCacheData<TKey, TValue> data,
            CancellationToken token)
        {
            try
            {
                _localCts.Token.ThrowIfCancellationRequested();
                return SafeAddOrReplaceFor(key, data, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidCache, "Unknown error", e);
            }
        }

        public override async Task Shutdown()
        {
            _localCts.Cancel();
            await _cacheSerializer.Shutdown();
            var fallback = Fallback;
            Thread.MemoryBarrier();
            if (fallback != null)
            {
                await fallback.Shutdown();
            }
        }

        private void PopulateCacheWithMerging(CacheData<TKey, TValue> data, BlockingCollection<TMap> collection,
            CancellationToken token, CancellationTokenSource cts)
        {
            try
            {
                foreach (var map in collection.GetConsumingEnumerable(token))
                {
                    var key = map.Key;
                    TValue outValue;
                    if (data.HasKey(key, out outValue))
                    {
                        TValue mergedValue;
                        if (map.MergeCurrentDataForKeyConflict(outValue, out mergedValue))
                        {
                            data.AddForced(key, mergedValue);
                        }
                        token.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        var value = map.Value;
                        data.AddForced(key, value);
                    }
                    _mapPool.Instance = map;
                }
            }
            finally
            {
                cts.Cancel();
            }
        }

        private void PopulateCacheWithoutMerging(CacheData<TKey, TValue> data, BlockingCollection<TMap> collection,
            CancellationToken token, CancellationTokenSource cts)
        {
            try
            {
                foreach (var map in collection.GetConsumingEnumerable(token))
                {
                    var key = map.Key;
                    data.AddForced(key, default(TValue));
                    _mapPool.Instance = map;
                }
            }
            catch
            {
                cts.Cancel();
                throw;
            }
        }

        private Task<bool> SafeAddOrReplaceFor(TKey key, IAsyncCacheData<TKey, TValue> data, CancellationToken token)
        {
            return data.ValueTypeCanBeMerged
                ? SafeAddOrReplaceWithMerging(key, data, token)
                : SafeAddOrReplaceWithoutMerging(key, data, token);
        }

        private async Task<bool> SafeAddOrReplaceWithoutMerging(TKey key, IAsyncCacheData<TKey, TValue> data,
            CancellationToken token)
        {
            var hasRow = await _keyBasedFetcher.HasRowAsync(key, token).ConfigureAwait(false);
            if (!hasRow)
            {
                return false;
            }
            data.AddOrReplace(key, default(TValue));
            return true;
        }

        private async Task<bool> SafeAddOrReplaceWithMerging(TKey key, IAsyncCacheData<TKey, TValue> data,
            CancellationToken token)
        {
            var dbdata = await _keyBasedFetcher.GetAsync(key, token).ConfigureAwait(false);
            if (dbdata == null || dbdata.Count == 0)
            {
                return false;
            }
            TValue value;
            if (dbdata.Count == 1)
            {
                value = dbdata[0].Value;
                data.AddOrReplace(key, value);
            }
            else
            {
                value = dbdata[0].Value;
                for (var i = 1; i < dbdata.Count; i++)
                {
                    TValue outVal;
                    if (dbdata[i].MergeCurrentDataForKeyConflict(value, out outVal))
                    {
                        value = outVal;
                    }
                }
                data.AddOrReplace(key, value);
            }
            return true;
        }
    }
}