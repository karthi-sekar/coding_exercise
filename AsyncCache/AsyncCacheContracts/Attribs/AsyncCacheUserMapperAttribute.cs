using System;

namespace AsyncCacheContract.Attribs
{
    /// <summary>
    /// This attribute should be applied only on Derived classes of DbMap abstract class.
    /// If used anywhere else, it gives no value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AsyncCacheUserMapperAttribute : Attribute
    {
        /// <summary>
        /// Static function name for actual data map.
        /// </summary>
        public string UserMapForActualData { get; private set; }

        /// <summary>
        /// Static function name for additional data map.
        /// </summary>
        public string UserMapForAdditionalData { get; private set; }

        /// <summary>
        /// Default Ctor.
        /// <para>The methods signature MUST be:
        /// <para>
        /// private/internal static IReadOnlyDictionary&lt;string, Delegate&gt; SomeMethodName()
        /// {
        ///    return new IReadOnlyDictionary&lt;string, Delegate&gt;
        ///    {
        ///        {
        ///            "StringType_DbColumnName",
        ///            new PropertySetter&lt;This_Class, string&gt;(
        ///                (obj, value) =&gt; obj.StringType_DbMapped_Prop = value)
        ///        },
        ///        {
        ///            "IntegerType_DbColumnName",
        ///            new PropertySetter&lt;This_Class, int&gt;(
        ///                (obj, value) =&gt; obj.IntegerType_DbMapped_Prop = value)
        ///        },
        ///        {
        ///            "DateTimeType_DbColumnName",
        ///            new PropertySetter&lt;This_Class, DateTime&gt;(
        ///                (obj, value) =&gt; obj.DateTimeType_DbMapped_Prop = value)
        ///        },
        ///        ...
        ///        ...
        ///        ... and so on...
        ///    };
        /// }
        /// </para></para></summary>
        /// <param name="staticMethodForActualData">Name of the Static method (INTERNAL or PRIVATE) which
        /// returns dictionary (for ActualQuery) whose:
        /// <para>Keys are dbcolumn names (as supplied for AsyncCacheAttribute) of actual query</para>
        /// <para>Values are delegates of type PropertySetter (defined in DbMap).</para>
        /// </param>
        /// <param name="staticMethodForAdditionalData">Name of the Static method (INTERNAL or PRIVATE) which 
        /// returns dictionary (for AdditionalData, When Cache config has AdType = AdditionalData.Database) whose:
        /// <para>Keys are dbcolumn names (as supplied for AsyncCacheAttribute) of additional data query</para>
        /// <para>Values are delegates of type PropertySetter (defined in DbMap).</para></param>
        public AsyncCacheUserMapperAttribute(string staticMethodForActualData,
            string staticMethodForAdditionalData)
        {
            UserMapForActualData = staticMethodForActualData;
            UserMapForAdditionalData = staticMethodForAdditionalData;
        }
    }
}