using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using AsyncCacheContract.Cache;
using log4net;

namespace AsyncCache.Lib.DbRelated
{
    internal sealed class AsyncDbDataFetcher<TMap, TKey, TValue> : IAsyncDbDataFetcher<TMap>,
        IDbDataFetcher<TMap, TKey, TValue> where TMap : DbMap<TKey, TValue>, new()
    {
        private readonly IConnectionMgr _connectionMgr;
        private readonly string _opname;
        private readonly string _opnameWithKey;
        private readonly string _query;
        private readonly Action<TMap, DbDataReader, int>[] _dbcolumnToPropertySetter;
        private readonly ObjectPool<TMap> _mapPool;
        private readonly Func<TKey, string> _keyBasedQueryGenerator;
        private readonly ILog _logger;

        public AsyncDbDataFetcher(IConnectionMgr connectionMgr, string opname, string query,
            Action<TMap, DbDataReader, int>[] dbcolumnToPropertySetter, ObjectPool<TMap> mapPool,
            Func<TKey, string> keyBasedQueryGenerator, ILog logger)
        {
            _connectionMgr = connectionMgr;
            _opname = $"{opname}-load";
            _opnameWithKey = $"{opname}-load-key";
            _query = query;
            _dbcolumnToPropertySetter = dbcolumnToPropertySetter;
            _mapPool = mapPool;
            _keyBasedQueryGenerator = keyBasedQueryGenerator;
            _logger = logger;
        }

        async Task IAsyncDbDataFetcher<TMap>.FillAsync(BlockingCollection<TMap> collection, CancellationToken token)
        {
            try
            {
                var conn = await _connectionMgr.GetConnectionAsync(_opname, token).ConfigureAwait(false);
                using (conn)
                {
                    using (var comm = conn.GetCommand)
                    {
                        comm.CommandText = _query;
                        comm.CommandType = CommandType.Text;

                        var reader = await comm.ExecuteReaderAsync(token).ConfigureAwait(false);
                        using (reader)
                        {
                            while (reader.Read())
                            {
                                var instance = _mapPool.Instance;
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    _dbcolumnToPropertySetter[i](instance, reader, i);
                                }
                                collection.Add(instance, token);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"({_opname}) Db Error", e);
                throw;
            }
        }

        async Task<IList<TMap>> IDbDataFetcher<TMap, TKey, TValue>.GetAsync(TKey key, CancellationToken token)
        {
            try
            {
                var conn = await _connectionMgr.GetConnectionAsync(_opnameWithKey, token).ConfigureAwait(false);
                using (conn)
                {
                    using (var comm = conn.GetCommand)
                    {
                        comm.CommandText = _keyBasedQueryGenerator(key);
                        comm.CommandType = CommandType.Text;

                        var reader = await comm.ExecuteReaderAsync(token).ConfigureAwait(false);
                        using (reader)
                        {
                            var items = new List<TMap>();
                            while (reader.Read())
                            {
                                token.ThrowIfCancellationRequested();
                                var instance = new TMap();
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    _dbcolumnToPropertySetter[i](instance, reader, i);
                                }
                                items.Add(instance);
                            }
                            return items;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"({_opnameWithKey}) Db Error", e);
                throw;
            }
        }

        async Task<bool> IDbDataFetcher<TMap, TKey, TValue>.HasRowAsync(TKey key, CancellationToken token)
        {
            try
            {
                var conn = await _connectionMgr.GetConnectionAsync(_opnameWithKey, token).ConfigureAwait(false);
                using (conn)
                {
                    using (var comm = conn.GetCommand)
                    {
                        comm.CommandText = _keyBasedQueryGenerator(key);
                        comm.CommandType = CommandType.Text;

                        var reader = await comm.ExecuteReaderAsync(token).ConfigureAwait(false);
                        using (reader)
                        {
                            return reader.HasRows;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"({_opnameWithKey}) Db Error", e);
                throw;
            }
        }
    }
}