namespace AsyncCacheContract.Enums
{
    /// <summary>
    /// Additional data nature for Asyc cache.
    /// </summary>
    public enum AdditionalData
    {
        /// <summary>
        /// Additional data from database.
        /// </summary>
        Database = 0,
        /// <summary>
        /// Additional data from local file (filename should be ActualAsyncCacheName_AD.xml)
        /// </summary>
        LocalFile,
        /// <summary>
        /// When there is no additional data
        /// </summary>
        None
    }
}