using System;
using System.Collections.Generic;
using System.IO;

namespace DotNetObjectsExt
{
    public static class ErrorRelated
    {
        public static void ThrowIfNullOrNonExisting(this DirectoryInfo di, string paramName)
        {
            if (di == null)
            {
                throw new ArgumentException($"{paramName} is null");
            }
            di.Refresh();
            if (!di.Exists)
            {
                throw new ArgumentException($"{paramName} (value:{di.FullName}) is non-existing.");
            }
        }

        public static void ThrowIfNull(this object obj, string paramName)
        {
            if (obj == null)
            {
                throw new ArgumentException($"{paramName} is null");
            }
        }

        public static void ThrowIfNullOrWhiteSpace(this string val, string paramName)
        {
            if (val.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($"{paramName} IsNullOrWhiteSpace");
            }
        }

        public static void ThrowIfNullOrEmpty<T>(this ICollection<T> obj, string paramName)
        {
            if (obj == null || obj.Count == 0)
            {
                throw new ArgumentException($"{paramName} IsNullOrEmpty");
            }
        }

        public static void ThrowIfNegative(this int val, string paramName)
        {
            val.ThrowIfLessThan(0, paramName);
        }

        public static void ThrowIfLessThan(this int val, int threshold, string paramName)
        {
            if (val < threshold)
            {
                throw new ArgumentException($"{paramName} (value: {val}) is less than {threshold}");
            }
        }

        public static void ThrowIfGreaterThan(this int val, int threshold, string paramName)
        {
            if (val > threshold)
            {
                throw new ArgumentException($"{paramName} (value: {val}) is greater than {threshold}");
            }
        }

        public static void ThrowWhenNotInRange(this int val, int lowerIncl, int upperIncl, string paramName)
        {
            if (val < lowerIncl || val > upperIncl)
            {
                throw new ArgumentException($"{paramName} (value:{val}) not in range:{lowerIncl}-{upperIncl}");
            }
        }

        public static void ThrowIfHasKey<TK,TV>(this Dictionary<TK,TV> dict, TK key, string paramName)
        {
            if (dict.ContainsKey(key))
            {
                throw new ArgumentException($"{paramName} (key: {key}) repeated.");
            }
        }
    }
}