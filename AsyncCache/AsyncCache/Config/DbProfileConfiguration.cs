using System.Configuration;
using AsyncCacheContract.Config;
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable 1591

namespace AsyncCache.Config
{
    public class DbProfileElementList : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DbProfileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IDbConfigProfile) element).Key;
        }
    }

    public class DbProfileElement : ConfigurationElement, IDbConfigProfile
    {
        [ConfigurationProperty("Key", IsRequired = true)]
        public string Key => (string) this["Key"];

        [ConfigurationProperty("ConnectionType", IsRequired = true)]
        public string ConnectionType => (string) this["ConnectionType"];

        [ConfigurationProperty("ConnectionString", IsRequired = true)]
        public string ConnectionString => (string) this["ConnectionString"];
    }
}