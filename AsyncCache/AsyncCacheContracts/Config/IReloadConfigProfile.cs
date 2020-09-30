using AsyncCacheContract.Enums;

namespace AsyncCacheContract.Config
{
    /// <summary>
    /// Reload profile config def.
    /// </summary>
    public interface IReloadConfigProfile
    {
        /// <summary>
        /// Unique name of the profile
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Hour component of the day for AtGivenTime (Range:0-23).
        /// OR
        /// Hour component of the duration for Interval (Range:0-167).
        /// </summary>
        int Hour { get; }

        /// <summary>
        /// Minute component of the day for AtGivenTime (range 0-59).
        /// OR
        /// Minute component of the duration for Interval (range 0-59).
        /// </summary>
        int Minute { get; }

        /// <summary>
        /// Reload type for reload schedule.
        /// </summary>
        ReloadType Type { get; }
    }
}