using System.Threading.Tasks;

namespace AsyncCache.Contracts
{
    internal interface ICacheDataSerializer
    {
        /// <summary>
        /// Thread safe at file level.
        /// </summary>
        Task Serialize<T>(T obj);

        /// <summary>
        /// Thread safe at file level.
        /// </summary>
        Task<T> DeserializeAsync<T>();
    }
}