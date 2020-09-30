using System.Configuration;
using AsyncCacheContract;
using AsyncCacheContract.Config;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable 1591

namespace AsyncCache.Config
{
    public class CacheElementList : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CacheElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ICacheConfigProfile) element).ConcreteType;
        }
    }

    public class CacheElement : ConfigurationElement, ICacheConfigProfile
    {
        [ConfigurationProperty("ConcreteType", IsRequired = true)]
        public string ConcreteType => (string) this["ConcreteType"];

        [ConfigurationProperty("Active", IsRequired = true)]
        public bool Active => (bool) this["Active"];

        [ConfigurationProperty("SerializationProfileKey", IsRequired = true)]
        public string SerializationProfileKey => (string) this["SerializationProfileKey"];

        [ConfigurationProperty("ReloadProfileKey", IsRequired = true)]
        public string ReloadProfileKey => (string) this["ReloadProfileKey"];

        [ConfigurationProperty("DbProfileKey", IsRequired = true)]
        public string DbProfileKey => (string) this["DbProfileKey"];

        [ConfigurationProperty("AdTypeEnum", IsRequired = true)]
        public string AdTypeEnum => (string) this["AdTypeEnum"];

        [ConfigurationProperty("AdDbProfile", IsRequired = true)]
        public string AdDbProfileKey => (string) this["AdDbProfile"];

        [ConfigurationProperty("GoToDbForMissingKey", IsRequired = true)]
        public bool GoToDbForMissingKey => (bool) this["GoToDbForMissingKey"];

        [ConfigurationProperty("PartitionCount", IsRequired = true)]
        public int PartitionCount => (int)this["PartitionCount"];

        [ConfigurationProperty("KeyIsValue", IsRequired = true)]
        public bool KeyIsValue => (bool)this["KeyIsValue"];

        [ConfigurationProperty("ReloadFromFileFirst", IsRequired = true)]
        public bool ReloadFromFileFirst => (bool)this["ReloadFromFileFirst"];

        public AdditionalData AdType
        {
            get
            {
                AdditionalData type;
                if (AdTypeEnum.TryToEnum(out type))
                {
                    return type;
                }
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                    $"{nameof(AdTypeEnum)} (value:{AdTypeEnum}) to Enum (Type:{typeof(AdditionalData).FullName})");
            }
        }
    }
}