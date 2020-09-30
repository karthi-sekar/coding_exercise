namespace AsyncCacheContract.Enums
{
    /// <summary>
    /// Enum values for AsyncCacheException.
    /// </summary>
    public enum AsyncCacheErrorCode
    {
        /// <summary>
        /// When config is NOT valid
        /// </summary>
        InvalidConfig,
        /// <summary>
        /// When cache instance is invalid somehow
        /// </summary>
        InvalidCache,
        /// <summary>
        /// When map defintion is invalid
        /// </summary>
        InvalidMap,
        /// <summary>
        /// When implementation is invalid
        /// </summary>
        InvalidImplementation
    }
}