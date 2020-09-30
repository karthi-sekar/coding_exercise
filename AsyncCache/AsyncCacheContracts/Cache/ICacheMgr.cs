using System;
using System.Collections.Generic;

namespace AsyncCacheContract.Cache
{
    /// <summary>
    /// Interface for cache manager.
    /// </summary>
    public interface ICacheMgr
    {
        /// <summary>
        /// Initializes/schedules all caches.
        /// </summary>
        void Init();

        /// <summary>
        /// Stops/Disposes all caches.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Gets all initialized instances of caches (as defined in config).
        /// <para>Dic6tionary Key is the type as mentioned in Config and value is the instance.</para>
        /// <para>If some cache were Inactive (Active = false in config), then value is null</para>
        /// </summary>
        IDictionary<Type, object> Caches { get; }
    }
}