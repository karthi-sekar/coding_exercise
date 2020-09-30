using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using AsyncCacheContract.Cache;
using AsyncCacheContract.Enums;

namespace AsyncCache.Helpers
{
    internal sealed class PropertySetterBuilder<TMap>
    {
        public bool IsValidType(PropertyInfo propertyInfo)
        {
            return _validTypes.Contains(propertyInfo.PropertyType);
        }

        public Delegate ToDelegate(PropertyInfo propertyInfo)
        {
            Func<PropertyInfo, Delegate> outVal;
            if (_propertyInfoBasedDelegates.TryGetValue(propertyInfo.PropertyType, out outVal))
            {
                return outVal(propertyInfo);
            }
            throw new InvalidOperationException($"Type ({propertyInfo.PropertyType.Name}) not recognized.");
        }

        public Action<TMap, DbDataReader, int> StringSetter(CacheStringConversion conversionType, Delegate setter, string columnName)
        {
            return StaticCalls.GetStringBinder<TMap>(setter, conversionType, columnName);
        }

        public Action<TMap, DbDataReader, int> StringSetter(Type dbcolType, CacheStringConversion conversionType,
            Delegate setter, string columnName)
        {
            Func<Delegate, string, Action<TMap, DbDataReader, int>> outFunc;
            return _stringerBinders.TryGetValue(dbcolType, out outFunc)
                ? outFunc(setter, columnName)
                : StringSetter(conversionType, setter, columnName);
        }

        public Action<TMap, DbDataReader, int> Setter(Type propertyType, Delegate setter, string columnName)
        {
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Func<Delegate, string, Action<TMap, DbDataReader, int>> outFunc;
                if (_nullableBinders.TryGetValue(propertyType, out outFunc))
                {
                    return outFunc(setter, columnName);
                }
            }
            else
            {
                Func<Delegate, string, Action<TMap, DbDataReader, int>> outFunc;
                if (_binders.TryGetValue(propertyType, out outFunc))
                {
                    return outFunc(setter, columnName);
                }
            }
            throw new InvalidOperationException($"Unable to find setter for type:{propertyType.Name}");
        }

        public Action<TMap, DbDataReader, int> Setter(Type dbcolType, Type propertyType, Delegate setter,
            string columnName, string propertyname)
        {
            if ((dbcolType == propertyType) || (propertyType == typeof(object)) ||
                (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                    propertyType.GenericTypeArguments[0] == dbcolType))
            {
                return Setter(propertyType, setter, columnName);
            }
            throw new InvalidOperationException($"Type mismatch. DbType (name:{columnName}," +
                $"type:{dbcolType.Name}) vs property-type (name:{propertyname},type:{propertyType.Name}).");
        }

        public string ValidTypes
        {
            get
            {
                var strBuilder = new StringBuilder();
                strBuilder.AppendLine();

                foreach (var validType in _validTypes)
                {
                    if (validType.IsGenericType && validType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        strBuilder.AppendLine(validType.GenericTypeArguments[0].Name + "?");
                    }
                    else
                    {
                        strBuilder.AppendLine(validType.Name);
                    }
                }

                return strBuilder.ToString();
            }
        }

        private readonly HashSet<Type> _validTypes = new HashSet<Type>
        {
            typeof(object),
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(long),
            typeof(int),
            typeof(double),
            typeof(short),
            typeof(byte),
            typeof(bool),
            typeof(char),
            typeof(float),
            typeof(Guid),
            typeof(decimal?),
            typeof(DateTime?),
            typeof(long?),
            typeof(int?),
            typeof(double?),
            typeof(short?),
            typeof(byte?),
            typeof(bool?),
            typeof(char?),
            typeof(float?),
            typeof(Guid?)
        };

        private readonly Dictionary<Type, Func<PropertyInfo, Delegate>> _propertyInfoBasedDelegates = new Dictionary
            <Type, Func<PropertyInfo, Delegate>>
        {
            {
                typeof(object), pi => new PropertySetter<TMap, object>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(string), pi => new PropertySetter<TMap, string>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(decimal), pi => new PropertySetter<TMap, decimal>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(DateTime), pi => new PropertySetter<TMap, DateTime>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(long), pi => new PropertySetter<TMap, long>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(int), pi => new PropertySetter<TMap, int>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(double), pi => new PropertySetter<TMap, double>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(short), pi => new PropertySetter<TMap, short>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(byte), pi => new PropertySetter<TMap, byte>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(bool), pi => new PropertySetter<TMap, bool>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(char), pi => new PropertySetter<TMap, char>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(float), pi => new PropertySetter<TMap, float>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(Guid), pi => new PropertySetter<TMap, Guid>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(decimal?), pi => new PropertySetter<TMap, decimal?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(DateTime?), pi => new PropertySetter<TMap, DateTime?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(long?), pi => new PropertySetter<TMap, long?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(int?), pi => new PropertySetter<TMap, int?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(double?), pi => new PropertySetter<TMap, double?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(short?), pi => new PropertySetter<TMap, short?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(byte?), pi => new PropertySetter<TMap, byte?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(bool?), pi => new PropertySetter<TMap, bool?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(char?), pi => new PropertySetter<TMap, char?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(float?), pi => new PropertySetter<TMap, float?>((o, v) => pi.SetValue(o, v, null))
            },
            {
                typeof(Guid?), pi => new PropertySetter<TMap, Guid?>((o, v) => pi.SetValue(o, v, null))
            }
        };

        private readonly Dictionary<Type, Func<Delegate, string, Action<TMap, DbDataReader, int>>> _binders = new Dictionary
            <Type, Func<Delegate, string, Action<TMap, DbDataReader, int>>>
        {
            {
                typeof(object), StaticCalls.GetObjectBinder<TMap>
            },
            {
                typeof(decimal), StaticCalls.GetDecimalBinder<TMap>
            },
            {
                typeof(DateTime), StaticCalls.GetDateTimeBinder<TMap>
            },
            {
                typeof(long), StaticCalls.GetLongBinder<TMap>
            },
            {
                typeof(int), StaticCalls.GetIntBinder<TMap>
            },
            {
                typeof(double), StaticCalls.GetDoubleBinder<TMap>
            },
            {
                typeof(short), StaticCalls.GetShortBinder<TMap>
            },
            {
                typeof(byte), StaticCalls.GetByteBinder<TMap>
            },
            {
                typeof(bool), StaticCalls.GetBooleanBinder<TMap>
            },
            {
                typeof(char), StaticCalls.GetCharBinder<TMap>
            },
            {
                typeof(float), StaticCalls.GetFloatBinder<TMap>
            },
            {
                typeof(Guid), StaticCalls.GetGuidBinder<TMap>
            }
        };

        private readonly Dictionary<Type, Func<Delegate, string, Action<TMap, DbDataReader, int>>> _nullableBinders = new Dictionary
            <Type, Func<Delegate, string, Action<TMap, DbDataReader, int>>>
        {
            {
                typeof(decimal?), StaticCalls.GetNullableDecimalBinder<TMap>
            },
            {
                typeof(DateTime?), StaticCalls.GetNullableDateTimeBinder<TMap>
            },
            {
                typeof(long?), StaticCalls.GetNullableLongBinder<TMap>
            },
            {
                typeof(int?), StaticCalls.GetNullableIntBinder<TMap>
            },
            {
                typeof(double?), StaticCalls.GetNullableDoubleBinder<TMap>
            },
            {
                typeof(short?), StaticCalls.GetNullableShortBinder<TMap>
            },
            {
                typeof(byte?), StaticCalls.GetNullableByteBinder<TMap>
            },
            {
                typeof(bool?), StaticCalls.GetNullableBooleanBinder<TMap>
            },
            {
                typeof(char?), StaticCalls.GetNullableCharBinder<TMap>
            },
            {
                typeof(float?), StaticCalls.GetNullableFloatBinder<TMap>
            },
            {
                typeof(Guid?), StaticCalls.GetNullableGuidBinder<TMap>
            }
        };

        private readonly Dictionary<Type, Func<Delegate, string, Action<TMap, DbDataReader, int>>> _stringerBinders = new Dictionary
            <Type, Func<Delegate, string, Action<TMap, DbDataReader, int>>>
        {
            {
                typeof(decimal), StaticCalls.GetDecimalToStringBinder<TMap>
            },
            {
                typeof(DateTime), StaticCalls.GetDateTimeToStringBinder<TMap>
            },
            {
                typeof(long), StaticCalls.GetLongToStringBinder<TMap>
            },
            {
                typeof(int), StaticCalls.GetIntToStringBinder<TMap>
            },
            {
                typeof(double), StaticCalls.GetDoubleToStringBinder<TMap>
            },
            {
                typeof(short), StaticCalls.GetShortToStringBinder<TMap>
            },
            {
                typeof(byte), StaticCalls.GetByteToStringBinder<TMap>
            },
            {
                typeof(bool), StaticCalls.GetBooleanToStringBinder<TMap>
            },
            {
                typeof(char), StaticCalls.GetCharToStringBinder<TMap>
            },
            {
                typeof(float), StaticCalls.GetFloatToStringBinder<TMap>
            },
            {
                typeof(Guid), StaticCalls.GetGuidToStringBinder<TMap>
            }
        };
    }
}