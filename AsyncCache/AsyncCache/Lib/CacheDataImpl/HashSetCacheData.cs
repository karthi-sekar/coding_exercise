using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Abstracts;
using AsyncCache.Contracts;

namespace AsyncCache.Lib.CacheDataImpl
{
    internal sealed class HashSetCacheData<TKey> : AsyncCacheData<TKey, TKey>
    {
        private readonly AutoResetEvent _serializationLock = new AutoResetEvent(true);
        private readonly HashSet<TKey>[] _data;
        private readonly ICacheDataSerializer _serializer;
        private bool _alreadySerialized;

        public HashSetCacheData(int partitionCount, IEqualityComparer<TKey> keyComparer, ICacheDataSerializer serializer)
            : base(partitionCount, keyComparer)
        {
            _serializer = serializer;
            _data = new HashSet<TKey>[TotalSync];
            for (var i = 0; i < TotalSync; i++)
            {
                _data[i] = new HashSet<TKey>(keyComparer);
            }
            _alreadySerialized = false;
        }

        internal override void AddForced(TKey key, TKey value)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            _data[IndexOf(key)].Add(key);
        }

        internal override bool HasKey(TKey key, out TKey existingValue)
        {
            existingValue = key;
            // ReSharper disable once InconsistentlySynchronizedField
            return _data[IndexOf(key)].Contains(key);
        }

        internal override void Merge(IAsyncCacheData<TKey, TKey> target, CancellationToken token)
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

        internal override bool ValueTypeCanBeMerged => false;

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
            var deserialData = await _serializer.DeserializeAsync<HashSet<TKey>[]>().ConfigureAwait(false);
            Parallel.For(0, deserialData.Length, new ParallelOptions
            {
                CancellationToken = token
            }, i =>
            {
                foreach (var value in deserialData[i])
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    AddOrReplace(value, value);
                }
            });
            token.ThrowIfCancellationRequested();
        }

        public override void AddOrReplace(IEnumerable<KeyValuePair<TKey, TKey>> data)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(IEnumerable<TKey> keys)
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
                        _data[index].Add(key);
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

        public override bool TryGet(TKey key, out TKey value)
        {
            value = key;
            var index = IndexOf(key);
            var syncRoot = SyncRoots[index];
            lock (syncRoot)
            {
                return _data[index].Contains(key);
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

        public override void AddOrReplace(TKey key, TKey value)
        {
            var lockTaken = false;
            try
            {
                lockTaken = _serializationLock.WaitOne();
                var index = IndexOf(key);
                var syncRoot = SyncRoots[index];
                lock (syncRoot)
                {
                    _data[index].Add(key);
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

        internal override AsyncCacheData<TKey, TKey> CreateEmpty()
        {
            return new HashSetCacheData<TKey>(TotalSync, Comparer, _serializer);
        }
    }
}