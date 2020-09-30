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
    public class ReloadProfileElementList : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ReloadProfileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IReloadConfigProfile) element).Key;
        }
    }

    public class ReloadProfileElement : ConfigurationElement, IReloadConfigProfile
    {
        [ConfigurationProperty("Key", IsRequired = true)]
        public string Key => (string) this["Key"];

        [ConfigurationProperty("Hour", IsRequired = true)]
        public int Hour => (int)this["Hour"];

        [ConfigurationProperty("Minute", IsRequired = true)]
        public int Minute => (int)this["Minute"];

        [ConfigurationProperty("TypeEnum", IsRequired = true)]
        public string TypeEnum => (string)this["TypeEnum"];

        public ReloadType Type
        {
            get
            {
                ReloadType type;
                if (TypeEnum.TryToEnum(out type))
                {
                    return type;
                }
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidConfig,
                    $"{nameof(TypeEnum)} (value:{TypeEnum}) to Enum (Type:{typeof(ReloadType).FullName})");
            }
        }
    }
}