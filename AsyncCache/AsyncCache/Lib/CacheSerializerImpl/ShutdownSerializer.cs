using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;

namespace AsyncCache.Lib.CacheSerializerImpl
{
    internal sealed class ShutdownSerializer : CacheSerializer
    {
        private ISerializableCache _cache;

        public override Task SerializeAsync(ISerializableCache cache)
        {
            // If return value is null, it means it is the first time
            //thus we serialize else we do nothing... Shutdown will do the job.
            return Interlocked.Exchange(ref _cache, cache) == null
                ? cache.Serialize()
                : Task.CompletedTask;
        }

        public override Task Shutdown()
        {
            var cache = Volatile.Read(ref _cache);
            return cache != null
                ? cache.Serialize()
                : Task.CompletedTask;
        }
    }
}