using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;

namespace AsyncCache.Lib.CacheSerializerImpl
{
    internal sealed class NeverSerializer : CacheSerializer
    {
        public override Task SerializeAsync(ISerializableCache cache)
        {
            //do nothing
            return Task.CompletedTask;
        }

        public override Task Shutdown()
        {
            //do nothing
            return Task.CompletedTask;
        }
    }
}