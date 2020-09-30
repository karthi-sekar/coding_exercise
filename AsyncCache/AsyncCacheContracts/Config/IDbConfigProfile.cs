namespace AsyncCacheContract.Config
{
    /// <summary>
    /// Db profile config def.
    /// </summary>
    public interface IDbConfigProfile
    {
        /// <summary>
        /// Unique name of the profile.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Name of DB as per TNS
        /// </summary>
        string ConnectionType { get; }

        /// <summary>
        /// User id for the connection.
        /// </summary>
        string ConnectionString { get; }
    }
}