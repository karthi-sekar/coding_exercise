using System.Collections.Concurrent;

namespace AsyncCache.Lib
{
    internal sealed class ObjectPool<T> where T : new()
    {
        private static readonly ConcurrentBag<T> Pool = new ConcurrentBag<T>();

        public T Instance
        {
            get
            {
                T outval;
                return Pool.TryTake(out outval) ? outval : new T();
            }
            set
            {
                Pool.Add(value);
            }
        }
    }
}