using System.Threading.Tasks;
using AsyncCache.Helpers;

namespace AsyncCache.Contracts
{
    internal interface IAsyncCache
    {
        Task InitAsync(InitInput input);
        Task Dispose();
    }
}