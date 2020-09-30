using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;
using log4net;

namespace AsyncCache.Lib.CacheDataWorkerImpl
{
    internal sealed class FileDataWorker<TKey, TValue> : CacheDataWorker<TKey, TValue>
    {
        private readonly string _cacheName;
        private readonly ILog _logger;
        private readonly CancellationTokenSource _localCts = new CancellationTokenSource();

        public FileDataWorker(string cacheName, ILog logger)
        {
            _cacheName = cacheName;
            _logger = logger;
        }

        public override Task Shutdown()
        {
            _localCts.Cancel();
            var fallback = Fallback;
            Thread.MemoryBarrier();
            return fallback != null ? fallback.Shutdown() : Task.CompletedTask;
        }

        public override Task<bool> AddOrReplaceForAsync(TKey key, IAsyncCacheData<TKey, TValue> data,
            CancellationToken token)
        {
            //We do NOT do key look up in FILE data.
            //if there is a fallback, we ask it
            var fallback = Fallback;
            Thread.MemoryBarrier();
            return fallback != null ? fallback.AddOrReplaceForAsync(key, data, token) : Task.FromResult(false);
        }

        protected override async Task LoadAsync(AsyncCacheData<TKey, TValue> emptyData, CancellationToken token)
        {
            _localCts.Token.ThrowIfCancellationRequested();
            using (var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_localCts.Token, token))
            {
                combinedCts.Token.ThrowIfCancellationRequested();
                try
                {
                    await LoadFromFileAsync(emptyData, combinedCts.Token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.Error($"({_cacheName}) Error during load from file.", e);
                    throw;
                }
            }
        }

        protected override Task SerializeAsync(ISerializableCache data)
        {
            //We do NOT serialize the data which we read from the file.
            return Task.CompletedTask;
        }

        private static Task LoadFromFileAsync(ISerializableCache data, CancellationToken token)
        {
            return data.DeserializeAndLoadAsync(token);
        }
    }
}