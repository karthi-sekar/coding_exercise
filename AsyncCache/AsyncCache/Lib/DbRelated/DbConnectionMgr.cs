using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using DotNetObjectsExt;
using log4net;

namespace AsyncCache.Lib.DbRelated
{
    internal sealed class DbConnectionMgr : IConnectionMgr
    {
        private readonly BlockingCollection<string> _dbConnectionLocker;
        private readonly ILog _logger;
        private readonly Func<string, string, DbConnection> _connectionGenerator;
        private readonly string _connectionType;
        private readonly string _connectionString;

        public DbConnectionMgr(BlockingCollection<string> concurrencyBlocker,
            Func<string, string, DbConnection> connectionGenerator, string connectionType, string connectionString,
            ILog logger)
        {
            _logger = logger;
            _dbConnectionLocker = concurrencyBlocker;
            _connectionGenerator = connectionGenerator;
            _connectionType = connectionType;
            _connectionString = connectionString;
        }

        public async Task<IConnection> GetConnectionAsync(string identifier, CancellationToken token)
        {
            var startTick = DateTime.UtcNow.Ticks;

            string connectionId;
            _dbConnectionLocker.TryTake(out connectionId, Timeout.Infinite, token);

            var actualConnection = _connectionGenerator(_connectionType, _connectionString);
            try
            {
                await actualConnection.OpenAsync(token).ConfigureAwait(false);
                _logger.Info($"{connectionId}:{identifier}. Time:{startTick.GetMillisecondDifference()} ms");
                return new DbConnect(actualConnection, connectionId, this);
            }
            catch (Exception e)
            {
                using (actualConnection)
                {
                    _logger.Error($"({connectionId}:{identifier}) DB Error.", e);
                }
                _dbConnectionLocker.Add(connectionId, CancellationToken.None);
                throw;
            }
        }

        private void ConnectionDisposed(string value, double totalTime)
        {
            _dbConnectionLocker.Add(value);
            _logger.InfoFormat($"{value} back:{totalTime} ms");
        }

        private sealed class DbConnect : IConnection
        {
            private DbConnection _dbConnection;
            private DbConnectionMgr _connectionMgr;
            private readonly long _startTick;
            private readonly string _connectionId;

            public DbConnect(DbConnection oracleConnection,
                string connectionId,
                DbConnectionMgr connectionMgr)
            {
                _dbConnection = oracleConnection;
                _connectionId = connectionId;
                _connectionMgr = connectionMgr;
                _startTick = DateTime.UtcNow.Ticks;
            }

            ~DbConnect()
            {
                Dispose(false);
            }

            private void Dispose(bool disposing)
            {
                if (!disposing)
                {
                    return;
                }
                if (_dbConnection != null)
                {
                    using (_dbConnection)
                    {

                    }
                    _dbConnection = null;
                }
                if (_connectionMgr == null)
                {
                    return;
                }
                _connectionMgr.ConnectionDisposed(_connectionId, _startTick.GetMillisecondDifference());
                _connectionMgr = null;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            public DbCommand GetCommand => _dbConnection.CreateCommand();

            public DbTransaction Transaction(IsolationLevel level)
            {
                return _dbConnection.BeginTransaction(level);
            }
        }
    }
}