using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;
using log4net;

namespace AsyncCache.Lib.CacheSerializerImpl
{
    internal sealed class ScheduledSerializer : CacheSerializer
    {
        private readonly ISerializationConfigProfile _config;
        private readonly ILog _logger;
        private readonly CancellationToken _token;
        private readonly string _cacheName;
        private readonly Task _reloadTask;
        private readonly CancellationTokenSource _localCts;
        private ISerializableCache _cache;

        public ScheduledSerializer(ISerializationConfigProfile config, ILog logger,
            string cacheName)
        {
            if (config.Type != SerializationType.AtGivenTime)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidImplementation,
                    $"{nameof(ScheduledSerializer)} supports only {SerializationType.AtGivenTime} option." +
                        $"Supplied type is {config.Type}");
            }
            _config = config;
            _logger = logger;
            _localCts = new CancellationTokenSource();
            _token = _localCts.Token;
            _cacheName = cacheName;
            _reloadTask = BeginSerializing();
        }

        private Task BeginSerializing()
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!_token.IsCancellationRequested)
                    {
                        await
                            Task.Delay((int) _config.Hour.DifferenceInMsForNextTimestampFromNow(_config.Minute), _token);
                        await SerializeSafe().ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Warn($"({_cacheName}) scheduled serialization is cancelled.");
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat($"({_cacheName}) scheduled serialization error.", e);
                }
            }, _token);
        }

        private async Task SerializeSafe()
        {
            var cache = Volatile.Read(ref _cache);
            if (cache != null)
            {
                try
                {
                    await cache.Serialize().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat($"(MAJOR)({_cacheName}) serialization error.", e);
                }
            }
        }

        public override Task SerializeAsync(ISerializableCache cache)
        {
            //If return value is null, it means it is the first time
            //thus we serialize else we do nothing... periodic scheduler will do the job.
            return Interlocked.Exchange(ref _cache, cache) == null
                ? cache.Serialize()
                : Task.CompletedTask;
        }

        public override async Task Shutdown()
        {
            _localCts.Cancel();
            using (_localCts)
            {
                await _reloadTask.ConfigureAwait(false);
            }
            var cache = Volatile.Read(ref _cache);
            if (cache != null)
            {
                await cache.Serialize().ConfigureAwait(false);
            }
        }
    }
}