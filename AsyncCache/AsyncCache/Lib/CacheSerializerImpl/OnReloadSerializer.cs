using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;

namespace AsyncCache.Lib.CacheSerializerImpl
{
    internal sealed class OnReloadSerializer : CacheSerializer
    {
        public override Task SerializeAsync(ISerializableCache cache)
        {
            return cache.Serialize();
        }

        public override Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }
}