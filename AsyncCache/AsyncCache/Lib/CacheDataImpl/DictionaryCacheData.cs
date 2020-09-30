using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;

namespace AsyncCache.Lib.CacheDataImpl
{
    internal sealed class DictionaryCacheData<TKey, TValue> : AsyncCacheData<TKey, TValue>
    {
        private readonly AutoResetEvent _serializationLock = new AutoResetEvent(true);
        private readonly Dictionary<TKey, TValue>[] _data;
        private readonly ICacheDataSerializer _serializer;
        private bool _alreadySerialized;

        public DictionaryCacheData(int partitionCount, IEqualityComparer<TKey> keyComparer,
            ICacheDataSerializer serializer, int initialCap = 0) : base(partitionCount, keyComparer)
        {
            _serializer = serializer;
            _data = new Dictionary<TKey, TValue>[TotalSync];
            for (var i = 0; i < TotalSync; i++)
            {
                _data[i] = new Dictionary<TKey, TValue>(initialCap, keyComparer);
            }
            _alreadySerialized = false;
        }

        private int Max
        {
            get
            {
                int max;
                var objectAtZero = SyncRoots[0];
                lock (objectAtZero)
                {
                    max = _data[0].Count;
                }
                for (var i = 1; i < TotalSync; i++)
                {
                    var objectAtI = SyncRoots[i];
                    lock (objectAtI)
                    {
                        max = Math.Max(max, _data[i].Count);
                    }
                }
                return max;
            }
        }

        internal override void AddForced(TKey key, TValue value)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            _data[IndexOf(key)][key] = value;
        }

        internal override bool HasKey(TKey key, out TValue existingValue)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            return _data[IndexOf(key)].TryGetValue(key, out existingValue);
        }

        internal override void Merge(IAsyncCacheData<TKey, TValue> target, CancellationToken token)
        {
            for (var i = 0; i < SyncRoots.Length; i++)
            {
                var syncRoot = SyncRoots[i];
                lock (syncRoot)
                {
                    token.ThrowIfCancellationRequested();
                    target.AddOrReplace(_data[i].AsEnumerable());
                }
            }
        }

        internal override void Clear()
        {
            for (var i = 0; i < TotalSync; i++)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                _data[i].Clear();
            }
        }

        internal override bool ValueTypeCanBeMerged => true;

        protected override async Task SerializeData()
        {
            if (!_alreadySerialized)
            {
                var lockTaken = false;
                try
                {
                    //Taking this lock to have thread safety at data level.
                    lockTaken = _serializationLock.WaitOne(Timeout.Infinite);
                    if (!_alreadySerialized)
                    {
                        await _serializer.Serialize(_data).ConfigureAwait(false);
                        _alreadySerialized = true;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _serializationLock.Set();
                    }
                }
            }
        }

        protected override async Task LoadFromDeserializeData(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var deserialData = await _serializer.DeserializeAsync<Dictionary<TKey, TValue>[]>().ConfigureAwait(false);
            Parallel.For(0, deserialData.Length, new ParallelOptions
            {
                CancellationToken = token
            }, i =>
            {
                foreach (var pair in deserialData[i])
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    AddOrReplace(pair.Key, pair.Value);
                }
            });
            token.ThrowIfCancellationRequested();
        }

        public override bool TryGet(TKey key, out TValue value)
        {
            var index = IndexOf(key);
            var syncRoot = SyncRoots[index];
            lock (syncRoot)
            {
                return _data[index].TryGetValue(key, out value);
            }
        }

        public override void TryRemove(IEnumerable<TKey> keys)
        {
            var lockTaken = false;
            try
            {
                lockTaken = _serializationLock.WaitOne();
                foreach (var key in keys)
                {
                    var index = IndexOf(key);
                    var syncRoot = SyncRoots[index];
                    lock (syncRoot)
                    {
                        _data[index].Remove(key);
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _serializationLock.Set();
                }
            }
        }

        public override bool TryRemove(TKey key)
        {
            var lockTaken = false;
            try
            {
                lockTaken = _serializationLock.WaitOne();
                var index = IndexOf(key);
                var syncRoot = SyncRoots[index];
                lock (syncRoot)
                {
                    return _data[index].Remove(key);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _serializationLock.Set();
                }
            }
        }

        public override void AddOrReplace(IEnumerable<TKey> keys)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(TKey key, TValue value)
        {
            var lockTaken = false;
            try
            {
                lockTaken = _serializationLock.WaitOne();
                var index = IndexOf(key);
                var syncRoot = SyncRoots[index];
                lock (syncRoot)
                {
                    _data[index][key] = value;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _serializationLock.Set();
                }
            }
        }

        public override void AddOrReplace(IEnumerable<KeyValuePair<TKey, TValue>> data)
        {
            var lockTaken = false;
            try
            {
                lockTaken = _serializationLock.WaitOne();
                foreach (var pair in data)
                {
                    var index = IndexOf(pair.Key);
                    var syncRoot = SyncRoots[index];
                    lock (syncRoot)
                    {
                        _data[index][pair.Key] = pair.Value;
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _serializationLock.Set();
                }
            }
        }

        internal override AsyncCacheData<TKey, TValue> CreateEmpty()
        {
            return new DictionaryCacheData<TKey, TValue>(TotalSync, Comparer, _serializer, Max);
        }
    }
}