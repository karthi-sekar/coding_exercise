namespace AsyncCacheContract.Enums
{
    /// <summary>
    /// Serialization type for AsycnCache.
    /// </summary>
    public enum SerializationType
    {
        /// <summary>
        /// First reload + at specific time (based on config Hour/Minute)
        /// </summary>
        AtGivenTime = 0,
        /// <summary>
        /// First reload + each time cache is reloaded.
        /// </summary>
        EveryReload,
        /// <summary>
        /// No Serialization at all (not even first time).
        /// </summary>
        Never,
        /// <summary>
        /// First reload + when service is shutting down.
        /// </summary>
        AtShutdown
    }
}