using System.Threading;
using System.Threading.Tasks;

namespace AsyncCache.Contracts
{
    internal interface IReloadable
    {
        Task Reload(CancellationToken token);
        bool ReloadOnceImmediate { get; set; }
    }
}