namespace AsyncCacheContract.Cache
{
    /// <summary>
    /// Base class for cache lookup only (all caching mechanism must implement it).
    /// </summary>
    /// <typeparam name="TKey">Type of Key.</typeparam>
    /// <typeparam name="TValue">Type of Value.</typeparam>
    public abstract class Cache<TKey, TValue>
    {
        /// <summary>
        /// Returns true with AsyncCacheValue instance when key is in cache. Else returns false.
        /// <para>Do NOT call this method and then check for out value for nullness. Good code should make 
        /// decisions based on bool output.</para>
        /// <para>When AsyncCache config has KeyIsValue = True (because KEY itself is value), out value is ALWAYS key-value itself.
        /// Thus make the decision based on boolean output.</para>
        /// <para>KeyIsValue = False, and output is True, then out value is NOT Null.</para>
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="value">Out value. When AsyncCache config has KeyIsValue = True (because KEY itself is value),
        /// out value is ALWAYS key value. Thus make the decision based on boolean output.</param>
        public abstract bool TryFind(TKey key, out TValue value);
    }
}