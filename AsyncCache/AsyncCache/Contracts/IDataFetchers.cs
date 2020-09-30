using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AsyncCacheContract.Cache;

namespace AsyncCache.Contracts
{
    internal interface IAsyncDbDataFetcher<TMap>
        where TMap : new()
    {
        Task FillAsync(BlockingCollection<TMap> collection, CancellationToken token);
    }

    internal interface IDbDataFetcher<TMap, in TKey, TValue>
        where TMap : DbMap<TKey,TValue>,  new()
    {
        Task<IList<TMap>> GetAsync(TKey key, CancellationToken token);
        Task<bool> HasRowAsync(TKey key, CancellationToken token);
    }
}