using System.Collections.Generic;

namespace AsyncCache.Contracts
{
    internal interface IAsyncCacheData<TKey, TValue>
    {
        bool ValueTypeCanBeMerged { get; }
        void AddOrReplace(TKey key, TValue value);
        void AddOrReplace(IEnumerable<KeyValuePair<TKey, TValue>> data);
        void AddOrReplace(IEnumerable<TKey> keys);
    }
}