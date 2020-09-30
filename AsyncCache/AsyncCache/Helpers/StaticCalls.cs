using System;
using System.Data.Common;
using System.Linq.Expressions;
using AsyncCacheContract.Cache;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;

namespace AsyncCache.Helpers
{
    internal static class StaticCalls
    {
        private static readonly int MinProcessor = Math.Max(4, Environment.ProcessorCount);

        public static int AdjustMaxConcurrency(int value)
        {
            return value == 0 ? MinProcessor : Math.Min(value, MinProcessor);
        }

        public static int AdjustPartitionCount(int value)
        {
            //Do not increase partitition size too much. Upper limit of #Processor is good enough.
            //with AT LEAST 4 partitions.
            return value == 0 ? MinProcessor : Math.Min(Math.Max(value, 4), MinProcessor);
        }

        public static Action<TMap, DbDataReader, int> GetObjectBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, object>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the object Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, object>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) => castedPropertySetter(obj, odr.GetValue(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetStringBinder<TMap>(Delegate propDelegate,
            CacheStringConversion conversionType, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            switch (conversionType)
            {
                case CacheStringConversion.None:
                    Expression<Action<TMap, DbDataReader, int>> expr1 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
                    return expr1.Compile();
                case CacheStringConversion.OnlyTrim:
                    Expression<Action<TMap, DbDataReader, int>> expr2 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString().Trim());
                    return expr2.Compile();
                case CacheStringConversion.ToLower:
                    Expression<Action<TMap, DbDataReader, int>> expr3 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj,
                                odr.IsDBNull(index) ? string.Empty : odr[index].ToString().ToLower());
                    return expr3.Compile();
                case CacheStringConversion.ToLowerWithTrim:
                    Expression<Action<TMap, DbDataReader, int>> expr4 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj,
                                odr.IsDBNull(index) ? string.Empty : odr[index].ToString().SafeTrimLower());
                    return expr4.Compile();
                case CacheStringConversion.ToUpper:
                    Expression<Action<TMap, DbDataReader, int>> expr5 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj,
                                odr.IsDBNull(index) ? string.Empty : odr[index].ToString().ToUpper());
                    return expr5.Compile();
                case CacheStringConversion.ToUpperWithTrim:
                    Expression<Action<TMap, DbDataReader, int>> expr6 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj,
                                odr.IsDBNull(index) ? string.Empty : odr[index].ToString().ToUpper().Trim());
                    return expr6.Compile();
                case CacheStringConversion.RemovedInterwordSpace:
                    Expression<Action<TMap, DbDataReader, int>> expr7 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj,
                                odr.IsDBNull(index) ? string.Empty : odr[index].ToString().Replace(" ", ""));
                    return expr7.Compile();
                case CacheStringConversion.RemovedInterwordSpaceAndToLower:
                    Expression<Action<TMap, DbDataReader, int>> expr8 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj,
                                odr.IsDBNull(index) ? string.Empty : odr[index].ToString().Replace(" ", "").ToLower());
                    return expr8.Compile();
                case CacheStringConversion.RemovedInterwordSpaceAndToUpper:
                    Expression<Action<TMap, DbDataReader, int>> expr9 =
                        (obj, odr, index) =>
                            castedPropertySetter(obj,
                                odr.IsDBNull(index) ? string.Empty : odr[index].ToString().Replace(" ", "").ToUpper());
                    return expr9.Compile();
                default:
                    throw new NotImplementedException($"{conversionType} not implemented.");
            }
        }

        public static Action<TMap, DbDataReader, int> GetDecimalBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, decimal>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the decimal Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, decimal>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(decimal) : odr.GetDecimal(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetByteBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, byte>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the byte Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, byte>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) => castedPropertySetter(obj, odr.IsDBNull(index) ? default(byte) : odr.GetByte(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetBooleanBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, bool>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the bool Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, bool>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) => castedPropertySetter(obj, !odr.IsDBNull(index) && odr.GetBoolean(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetCharBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, char>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the char Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, char>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) => castedPropertySetter(obj, odr.IsDBNull(index) ? default(char) : odr.GetChar(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetFloatBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, float>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the float Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, float>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(float) : odr.GetFloat(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetGuidBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, Guid>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the Guid Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, Guid>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) => castedPropertySetter(obj, odr.IsDBNull(index) ? default(Guid) : odr.GetGuid(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetDateTimeBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, DateTime>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the DateTime Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, DateTime>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(DateTime) : odr.GetDateTime(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetLongBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, long>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the long Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, long>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(long) : odr.GetInt64(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetIntBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, int>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the int Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, int>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) => castedPropertySetter(obj, odr.IsDBNull(index) ? default(int) : odr.GetInt32(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetDoubleBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, double>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the double Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, double>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(double) : odr.GetDouble(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetShortBinder<TMap>(Delegate propDelegate, string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, short>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the short Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, short>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(short) : odr.GetInt16(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableDecimalBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, decimal?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the  decimal? Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, decimal?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(decimal?) : odr.GetDecimal(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableByteBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, byte?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the byte? Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, byte?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(byte?) : odr.GetByte(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableBooleanBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, bool?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the bool? Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, bool?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(bool?) : odr.GetBoolean(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableCharBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, char?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the char? Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, char?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(char?) : odr.GetChar(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableFloatBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, float?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the float? Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, float?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(float?) : odr.GetFloat(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableGuidBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, Guid?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the Guid? Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, Guid?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(Guid?) : odr.GetGuid(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableDateTimeBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, DateTime?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the DateTime? Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, DateTime?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(DateTime?) : odr.GetDateTime(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableLongBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, long?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the long? Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, long?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(long?) : odr.GetInt64(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableIntBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, int?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException($"Unable to cast the int? Property setter (DbColumn:{columnName}) as " +
                    $"{typeof(PropertySetter<TMap, int?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(int?) : odr.GetInt32(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableDoubleBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, double?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the double? Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, double?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(double?) : odr.GetDouble(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetNullableShortBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, short?>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the short? Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, double?>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? default(short?) : odr.GetInt16(index));
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetDecimalToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetDateTimeToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetLongToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetIntToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetDoubleToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetShortToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetByteToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetBooleanToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetCharToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) => castedPropertySetter(obj, odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetFloatToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }

        public static Action<TMap, DbDataReader, int> GetGuidToStringBinder<TMap>(Delegate propDelegate,
            string columnName)
        {
            var castedPropertySetter = propDelegate as PropertySetter<TMap, string>;
            if (castedPropertySetter == null)
            {
                throw new InvalidCastException(
                    $"Unable to cast the String Property setter (DbColumn:{columnName}) as " +
                        $"{typeof(PropertySetter<TMap, string>).Name}. Given delegate type is: {propDelegate.GetType()}");
            }
            Expression<Action<TMap, DbDataReader, int>> expr =
                (obj, odr, index) =>
                    castedPropertySetter(obj, odr.IsDBNull(index) ? string.Empty : odr[index].ToString());
            return expr.Compile();
        }
    }
}