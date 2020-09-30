using AsyncCacheContract.Enums;

namespace AsyncCacheContract.Config
{
    /// <summary>
    /// Serialization profile def.
    /// </summary>
    public interface ISerializationConfigProfile
    {
        /// <summary>
        /// Unique name of the profile
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Hour component of the day for AtGivenTime (Range:0-23)
        /// </summary>
        int Hour { get; }

        /// <summary>
        /// Minute component of the day for AtGivenTime (Range:0-59)
        /// </summary>
        int Minute { get; }

        /// <summary>
        /// Serialization type for serialization schedule.
        /// </summary>
        SerializationType Type { get; }
    }
}