using System.Threading.Tasks;
using AsyncCache.Contracts;

namespace AsyncCache.Abstracts
{
    internal abstract class CacheSerializer
    {
        /// <summary>
        /// Thread safe on both File and Data.
        /// </summary>
        public abstract Task SerializeAsync(ISerializableCache cache);

        /// <summary>
        /// Thread safe on both File and Data.
        /// </summary>
        public abstract Task Shutdown();
    }
}