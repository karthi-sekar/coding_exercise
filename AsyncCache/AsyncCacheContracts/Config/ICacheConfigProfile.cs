using AsyncCacheContract.Enums;

namespace AsyncCacheContract.Config
{
    /// <summary>
    /// Cache config def.
    /// </summary>
    public interface ICacheConfigProfile
    {
        /// <summary>
        /// True to add cache in running solution. False to keep it inactive.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Type of actual AsyncCache implementation.
        /// </summary>
        string ConcreteType { get; }

        /// <summary>
        /// Profile key for the Serialization schedule.
        /// </summary>
        string SerializationProfileKey { get; }

        /// <summary>
        /// Profile key for the reload schedule.
        /// </summary>
        string ReloadProfileKey { get; }

        /// <summary>
        /// Profile key for DB details.
        /// </summary>
        string DbProfileKey { get; }

        /// <summary>
        /// Type of AdditionalData.
        /// </summary>
        AdditionalData AdType { get; }

        /// <summary>
        /// Profile key for Additional data db details (when AD is on DB).
        /// </summary>
        string AdDbProfileKey { get; }

        /// <summary>
        /// True to hit db when key is not found locally, else false.
        /// </summary>
        bool GoToDbForMissingKey { get; }

        /// <summary>
        /// Number of partition to keep for Cache data (0 for automatic partition)
        /// <para>This may improve memory consumption</para>
        /// </summary>
        int PartitionCount { get; }

        /// <summary>
        /// Set true when Key and Value are same from business perspective (eg. both are same string).
        /// <para>Improves memory consumption</para>
        /// </summary>
        bool KeyIsValue { get; }

        /// <summary>
        /// <para>If true, first data will be loaded from file. If file is not available or error occurs, it will
        /// try to get data from db, if error occurs again then it will be thrown back.</para>
        /// <para>If false, first data from DB will be recovered, if an error occurs then data will be looked into files.
        /// If file does NOT exists or error occurs, this error will be thrown back.</para>
        /// <para>This boolean appiled ONLY for the first time loading (at start-up). For regular reloading,
        /// data will be fetched from DB and if error occurs, it will be logged and old cache will be in use.</para>
        /// <para>Same applies to Additional Data, except, no error will be thrown back but logged.</para>
        /// </summary>
        bool ReloadFromFileFirst { get; }
    }
}