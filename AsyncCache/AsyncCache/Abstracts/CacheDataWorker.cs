using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using AsyncCacheContract;
using AsyncCacheContract.Enums;

namespace AsyncCache.Abstracts
{
    internal abstract class CacheDataWorker<TKey, TValue>
    {
        public async Task FreshLoadToAsync(AsyncCacheData<TKey, TValue> data, CancellationToken token)
        {
            try
            {
                data.Clear();
                await LoadAsync(data, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                await SerializeAsync(data).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (Fallback == null)
                {
                    throw new AsyncCacheException(AsyncCacheErrorCode.InvalidCache, "Unknown Error", e);
                }
                await Fallback.FreshLoadToAsync(data, token).ConfigureAwait(false);
            }
        }

        public CacheDataWorker<TKey, TValue> Fallback { protected get; set; }

        public abstract Task Shutdown();

        public abstract Task<bool> AddOrReplaceForAsync(TKey key, IAsyncCacheData<TKey, TValue> data, CancellationToken token);

        protected abstract Task LoadAsync(AsyncCacheData<TKey, TValue> emptyData, CancellationToken token);

        protected abstract Task SerializeAsync(ISerializableCache data);
    }
}