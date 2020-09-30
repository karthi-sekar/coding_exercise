using System.Threading;
using System.Threading.Tasks;

namespace AsyncCache.Contracts
{
    internal interface ISerializableCache
    {
        /// <summary>
        /// Thread safe at file level. Thread safe at data level.
        /// </summary>
        Task Serialize();

        /// <summary>
        /// Thread safe at file level. Thread safe at data level.
        /// </summary>
        Task DeserializeAndLoadAsync(CancellationToken token);
    }
}