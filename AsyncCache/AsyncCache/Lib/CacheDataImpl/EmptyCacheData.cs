using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;

namespace AsyncCache.Lib.CacheDataImpl
{
    internal sealed class EmptyCacheData<TKey, TValue> : AsyncCacheData<TKey, TValue>
    {
        public EmptyCacheData(int partitionCount, IEqualityComparer<TKey> keyComparer) : base(partitionCount, keyComparer)
        {
        }

        internal override void AddForced(TKey key, TValue value)
        {
            
        }

        internal override bool HasKey(TKey key, out TValue existingValue)
        {
            existingValue = default(TValue);
            return false;
        }

        internal override void Clear()
        {
            
        }

        internal override bool ValueTypeCanBeMerged => false;

        public override void AddOrReplace(TKey key, TValue value)
        {
            
        }

        public override void AddOrReplace(IEnumerable<KeyValuePair<TKey, TValue>> data)
        {
            
        }

        public override void AddOrReplace(IEnumerable<TKey> keys)
        {
            
        }

        public override bool TryGet(TKey key, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        public override bool TryRemove(TKey key)
        {
            return false;
        }

        public override void TryRemove(IEnumerable<TKey> keys)
        {
            
        }

        internal override void Merge(IAsyncCacheData<TKey, TValue> target, CancellationToken token)
        {
            
        }

        internal override AsyncCacheData<TKey, TValue> CreateEmpty()
        {
            return this;
        }

        protected override Task SerializeData()
        {
            return Task.CompletedTask;
        }

        protected override Task LoadFromDeserializeData(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}