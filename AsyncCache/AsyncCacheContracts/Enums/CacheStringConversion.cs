namespace AsyncCacheContract.Enums
{
    /// <summary>
    /// String conversion conventions.
    /// </summary>
    public enum CacheStringConversion
    {
        /// <summary>
        /// To keep DB string value as it (no change in case)
        /// </summary>
        None = 0,
        /// <summary>
        /// To convert the DB data as lower case string
        /// </summary>
        ToLower,
        /// <summary>
        /// To convert the DB data as upper case string
        /// </summary>
        ToUpper,
        /// <summary>
        /// Trim the string and convert to lower case
        /// </summary>
        ToLowerWithTrim,
        /// <summary>
        /// Trim the string and convert to upper case
        /// </summary>
        ToUpperWithTrim,
        /// <summary>
        /// Trim the string without any other modification
        /// </summary>
        OnlyTrim,
        /// <summary>
        /// Removes the spaces between words.
        /// </summary>
        RemovedInterwordSpace,
        /// <summary>
        /// Removes the spaces between words and converts string to lower
        /// </summary>
        RemovedInterwordSpaceAndToLower,
        /// <summary>
        /// Removes the spaces between words and converts string to upper
        /// </summary>
        RemovedInterwordSpaceAndToUpper,
    }
}