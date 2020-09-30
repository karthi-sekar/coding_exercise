using System.Threading;
using System.Threading.Tasks;

namespace AsyncCacheContract.Cache
{
    /// <summary>
    /// Interface must be implemented by cache which provides means to modify
    /// cache data on the fly.
    /// </summary>
    public interface IAlterableCache<in TKey, in TValue>
    {
        /// <summary>
        /// Removes given key from cache with associated data.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        bool TryRemove(TKey key);

        /// <summary>
        /// Forces the new value on the given key.
        /// </summary>
        /// <param name="key">Key instance</param>
        /// <param name="value">Value to force.</param>
        void AddOrReplace(TKey key, TValue value);

        /// <summary>
        /// Gets the key data from the DB and replces the existing entry in the cache.
        /// </summary>
        /// <param name="key">Key to look for</param>
        Task<bool> RefreshFromDb(TKey key);

        /// <summary>
        /// Refreshes whole cache from DB.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        Task RefreshFromDb(CancellationToken token);
    }
}