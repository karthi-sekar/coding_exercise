using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AsyncCacheContract.Config;
// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable 1591

namespace AsyncCache.Config
{
    public class AsyncCacheConfigSection : ConfigurationSection, IAsyncCacheConfig
    {
        [ConfigurationProperty("DbProfiles", IsRequired = true)]
        [ConfigurationCollection(typeof(DbProfileElement), AddItemName = "DbProfile")]
        public DbProfileElementList DbProfileElementList => (DbProfileElementList) this["DbProfiles"];

        [ConfigurationProperty("ReloadProfiles", IsRequired = true)]
        [ConfigurationCollection(typeof(ReloadProfileElement), AddItemName = "ReloadProfile")]
        public ReloadProfileElementList ReloadProfileElementList => (ReloadProfileElementList) this["ReloadProfiles"];

        [ConfigurationProperty("SerializationProfiles", IsRequired = true)]
        [ConfigurationCollection(typeof(SerializationProfileElement), AddItemName = "SerializationProfile")]
        public SerializationProfileElementList SerializationProfileElementList
            => (SerializationProfileElementList) this["SerializationProfiles"];

        [ConfigurationProperty("CacheProfiles", IsRequired = true)]
        [ConfigurationCollection(typeof(CacheElement), AddItemName = "CacheProfile")]
        public CacheElementList CacheElementList => (CacheElementList) this["CacheProfiles"];

        [ConfigurationProperty("CacheLocalFolder", IsRequired = true)]
        public string CacheLocalFolder => (string) base["CacheLocalFolder"];

        [ConfigurationProperty("MaxConcurrency", IsRequired = true)]
        public int MaxConcurrency => (int)base["MaxConcurrency"];

        public IList<IDbConfigProfile> DbProfiles => DbProfileElementList.Cast<IDbConfigProfile>().ToList();
        public IList<IReloadConfigProfile> ReloadProfiles => ReloadProfileElementList.Cast<IReloadConfigProfile>().ToList();

        public IList<ISerializationConfigProfile> SerializationProfiles
            => SerializationProfileElementList.Cast<ISerializationConfigProfile>().ToList();

        public IList<ICacheConfigProfile> CacheProfiles => CacheElementList.Cast<ICacheConfigProfile>().ToList();
    }
}