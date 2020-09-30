using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;

namespace AsyncCache.Abstracts
{
    internal abstract class CacheData<TKey, TValue>
    {
        protected readonly object[] SyncRoots;
        protected readonly int TotalSync;

        protected CacheData(int syncCount)
        {
            TotalSync = syncCount;
            SyncRoots = new object[syncCount];
            for (var i = 0; i < syncCount; i++)
            {
                SyncRoots[i] = new object();
            }
        }

        /// <summary>
        /// Not thread safe.
        /// </summary>
        internal abstract void AddForced(TKey key, TValue value);

        /// <summary>
        /// Not thread safe.
        /// </summary>
        internal abstract bool HasKey(TKey key, out TValue existingValue);

        /// <summary>
        /// Not thread safe.
        /// </summary>
        internal abstract void Clear();

        internal abstract bool ValueTypeCanBeMerged { get; }
    }

    internal abstract class AsyncCacheData<TKey, TValue> : CacheData<TKey, TValue>, IAsyncCacheData<TKey, TValue>,
        ISerializableCache
    {
        protected readonly IEqualityComparer<TKey> Comparer;

        protected AsyncCacheData(int partitionCount, IEqualityComparer<TKey> keyComparer) : base(partitionCount)
        {
            Comparer = keyComparer;
        }

        protected int IndexOf(TKey key)
        {
            return (Comparer.GetHashCode(key) & int.MaxValue) % TotalSync;
        }

        Task ISerializableCache.Serialize()
        {
            return SerializeData();
        }

        Task ISerializableCache.DeserializeAndLoadAsync(CancellationToken token)
        {
            return LoadFromDeserializeData(token);
        }

        protected abstract Task SerializeData();
        protected abstract Task LoadFromDeserializeData(CancellationToken token);

        bool IAsyncCacheData<TKey, TValue>.ValueTypeCanBeMerged => ValueTypeCanBeMerged;
        public abstract void AddOrReplace(TKey key, TValue value);
        public abstract void AddOrReplace(IEnumerable<KeyValuePair<TKey, TValue>> data);
        public abstract void AddOrReplace(IEnumerable<TKey> keys);
        public abstract bool TryGet(TKey key, out TValue value);
        public abstract bool TryRemove(TKey key);
        public abstract void TryRemove(IEnumerable<TKey> keys);
        internal abstract void Merge(IAsyncCacheData<TKey, TValue> target, CancellationToken token);
        internal abstract AsyncCacheData<TKey, TValue> CreateEmpty();
    }
}