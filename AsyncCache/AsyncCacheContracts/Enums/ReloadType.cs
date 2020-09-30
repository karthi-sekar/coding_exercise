namespace AsyncCacheContract.Enums
{
    /// <summary>
    /// Type of reloads for AsyncCache
    /// </summary>
    public enum ReloadType
    {
        /// <summary>
        /// At time interval as defined by Hour/Minute combination.
        /// </summary>
        Interval = 0,
        /// <summary>
        /// At specific time of the day (defined by Hour/Minute). Thus, ONCE per day.
        /// </summary>
        AtGivenTime
    }
}