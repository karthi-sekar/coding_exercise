using System;
using System.Globalization;

namespace DotNetObjectsExt
{
    public static class StringTryTo
    {
        private static readonly IFormatProvider DefaultFormatProvider = new CultureInfo("en-US");

        public static bool TryToEnum<T>(this string value, out T result, bool ignoreCase = true)
            where T : struct, IComparable, IFormattable, IConvertible
        {
            result = default(T);
            var typeEnum = typeof(T);
            return typeEnum.IsEnum && Enum.TryParse(value, ignoreCase, out result) && typeEnum.IsEnumDefined(result);
        }

        public static bool TryTo(this string value, out int result, IFormatProvider formatProvider = null,
            NumberStyles numStyle = NumberStyles.Any)
        {
            return int.TryParse(value, numStyle, formatProvider ?? DefaultFormatProvider, out result);
        }

        public static bool TryTo(this string value, out ulong result, IFormatProvider formProvider = null,
            NumberStyles numStyle = NumberStyles.Any)
        {
            return ulong.TryParse(value, numStyle, formProvider ?? DefaultFormatProvider, out result);
        }

        public static bool TryTo(this string value, out uint result, IFormatProvider formProvider = null,
            NumberStyles numStyle = NumberStyles.Any)
        {
            return uint.TryParse(value, numStyle, formProvider ?? DefaultFormatProvider, out result);
        }
    }
}