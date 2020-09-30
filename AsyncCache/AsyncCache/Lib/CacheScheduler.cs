using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using AsyncCache.Extensions;
using AsyncCacheContract.Config;
using DotNetObjectsExt;
using log4net;

namespace AsyncCache.Lib
{
    internal sealed class CacheScheduler
    {
        private readonly IReloadable _instance;
        private readonly IReloadConfigProfile _profile;
        private readonly ILog _logger;
        private readonly string _cacheName;
        private readonly Task _reloadTask;
        private readonly CancellationTokenSource _localCts;
        private readonly CancellationTokenSource _combinedCts;
        private readonly CancellationToken _token;

        public CacheScheduler(IReloadable reloadableInstance, IReloadConfigProfile profile, CancellationToken token,
            ILog logger, string cacheName)
        {
            reloadableInstance.ThrowIfNull($"{nameof(CacheScheduler)}.{nameof(reloadableInstance)}");
            profile.ReloadDelay();
            _localCts = new CancellationTokenSource();
            _combinedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _localCts.Token);
            _token = _combinedCts.Token;

            _instance = reloadableInstance;
            _profile = profile;
            _logger = logger;
            _cacheName = cacheName;
            _reloadTask = BeginReloading();
        }

        private Task BeginReloading()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (_instance.ReloadOnceImmediate)
                    {
                        await LoadSafe().ConfigureAwait(false);
                    }
                    while (!_token.IsCancellationRequested)
                    {
                        await Task.Delay(_profile.ReloadDelay(), _token);
                        await LoadSafe().ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.Warn($"({_cacheName}) scheduled reload is cancelled. No more reload will happen.");
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat($"({_cacheName}) scheduled reload error.", e);
                }
            }, _token);
        }

        private async Task LoadSafe()
        {
            try
            {
                await _instance.Reload(_token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.ErrorFormat($"(MAJOR)({_cacheName}) reload error.", e);
            }
        }

        public async Task Dispose()
        {
            _localCts.Cancel();
            using (_localCts)
            {
                using (_combinedCts)
                {
                    await _reloadTask.ConfigureAwait(false);
                }
            }
        }
    }
}