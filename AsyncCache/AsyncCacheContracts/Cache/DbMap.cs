namespace AsyncCacheContract.Cache
{
    /// <summary>
    /// Delegate which MUST be used in the user defined value setter dictionary.
    /// This dictionary will be recovered from a STATIC method of the concrete class of this class.
    /// The name of the method must be supplied by using AsyncCacheUserMapperAttribute
    /// on the class which implements this abstract class.
    /// <para>Dictionary must be KEYed by the name of the DBColumn and corresponding
    /// delegate must set the property associated with that DB column.</para>
    /// </summary>
    /// <param name="target">DbMap instance on which value will be set</param>
    /// <param name="value">value to be set</param>
    /// <typeparam name="TMap">Type of the DbMap.</typeparam>
    /// <typeparam name="T">Type of the input value (must be same as db property type).</typeparam>
    public delegate void PropertySetter<in TMap, in T>(TMap target, T value);

    /// <summary>
    /// Base class to map db values to its PUBLIC ONLY properties.
    /// <para>Do NOT use/pass the instances of this class for any OTHER purpose, as these
    /// instances would be recycled internally.</para>
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public abstract class DbMap<TKey, TValue>
    {
        /// <summary>
        /// Must return Key for the AsycnCache.
        /// <para>This property would be called once the object is initialized with DB values.</para>
        /// </summary>
        public abstract TKey Key { get; }

        /// <summary>
        /// Must return NON-Null Value for the AsycnCache.
        /// <para>If Null is returned then an exception will be thrown.</para>
        /// <para>This property would be called once the object is initialized with DB values.</para>
        /// <para>IMPORTANT:</para>
        /// <para>1. This property will NOT be called when KEY-conflict is detected.</para>
        /// <para>2. This method will NOT be called when AsyncCache config has  KeyIsValue = True
        /// (because KEY itself is value).</para>
        /// </summary>
        public abstract TValue Value { get; }

        /// <summary>
        /// This method will be called when KEY conflict is detected (same KEY is retrieved based on DB 
        /// mapped property values).
        /// <para>IMPORTANT:</para>
        /// <para>1. This method will NOT be called when AsyncCache config has 
        /// KeyIsValue = True (because KEY itself is value). And data will be rejected silently.</para>
        /// <para>2. Return true for ValueTypes (e.g. int, double etc) and Immutable types (e.g. string).
        /// returns false for mutable reference types (e.g. custom mutable classes).</para>
        /// <para>3. You need to merge the current data (i.e. this) with <paramref name="existingValue"/>.
        /// It is wise to NOT to call Value property inside this method as it would unnecessary create 
        /// GC objects. Instead, merge values based on DbMapped property values only.</para>
        /// </summary>
        /// <param name="existingValue">Value which already exists in the Cache for the comflicted key</param>
        /// <param name="replacementValue">Value to replace existing value. 
        /// <para>Replacement will take place if you return true else nothing will happen.</para></param>
        public abstract bool MergeCurrentDataForKeyConflict(TValue existingValue, out TValue replacementValue);
    }
}