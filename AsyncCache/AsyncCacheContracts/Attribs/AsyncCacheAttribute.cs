using System;
using AsyncCacheContract.Enums;

namespace AsyncCacheContract.Attribs
{
    /// <summary>
    /// Attribute to be placed on each custom INSTANCE property (can be private or internal BUT not PUBLIC and not STATIC) 
    /// of AsyncCacheValue class
    /// which requires DBColumn value to be mapped.
    /// </summary>
    /// <exception cref="AsyncCacheException">Thrown when DBType and property type does NOT match 
    /// (having string type on property will NOT throw exception, all DB values be simply converted to string)</exception>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AsyncCacheAttribute : Attribute
    {
        /// <summary>
        /// Default Ctor.
        /// <para>If DB has null string value, INSTANCE property (can be private or internal BUT not PUBLIC and not STATIC) 
        /// will contain string.empty value.</para>
        /// </summary>
        /// <param name="dbcolumnName">Name of DBColumn in the select query.</param>
        /// <param name="stringConversionType">This value has NO impact on NON-String type values.</param>
        /// <param name="additionalDataDbColumnName">Name of the db column in AdditionalDataQuery 
        /// (When Cache config has AdType = AdditionalData.Database).</param>
        public AsyncCacheAttribute(string dbcolumnName, CacheStringConversion stringConversionType,
            string additionalDataDbColumnName = null)
        {
            ConversionType = stringConversionType;
            DbColumnName = dbcolumnName;
            AdditionalDataDbColumnName = additionalDataDbColumnName;
        }

        /// <summary>
        /// Get Conversion type.
        /// </summary>
        public CacheStringConversion ConversionType { get; private set; }

        /// <summary>
        /// Get Db column name for actual data query.
        /// </summary>
        public string DbColumnName { get; private set; }

        /// <summary>
        /// Get Db column name for additional data query.
        /// </summary>
        public string AdditionalDataDbColumnName { get; private set; }
    }
}