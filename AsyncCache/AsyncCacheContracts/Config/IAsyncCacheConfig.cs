using System.Collections.Generic;

namespace AsyncCacheContract.Config
{
    /// <summary>
    /// Top level config wrapper def.
    /// </summary>
    public interface IAsyncCacheConfig
    {
        /// <summary>
        /// Folder where all serialized data will be kept.
        /// </summary>
        string CacheLocalFolder { get; }

        /// <summary>
        /// Max concurrency during cache reload.
        /// </summary>
        int MaxConcurrency { get; }

        /// <summary>
        /// Db profile collection.
        /// </summary>
        IList<IDbConfigProfile> DbProfiles { get; }

        /// <summary>
        /// Reload profile collection.
        /// </summary>
        IList<IReloadConfigProfile> ReloadProfiles { get; }

        /// <summary>
        /// Serialization profile collection.
        /// </summary>
        IList<ISerializationConfigProfile> SerializationProfiles { get; }

        /// <summary>
        /// Cache config collection.
        /// </summary>
        IList<ICacheConfigProfile> CacheProfiles { get; }
    }
}