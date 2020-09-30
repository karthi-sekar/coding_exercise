using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncCache.Contracts
{
    public interface IConnectionMgr
    {
        Task<IConnection> GetConnectionAsync(string reportId, CancellationToken token);
    }

    public interface IConnection : IDisposable
    {
        DbCommand GetCommand { get; }
        DbTransaction Transaction(IsolationLevel level);
    }
}