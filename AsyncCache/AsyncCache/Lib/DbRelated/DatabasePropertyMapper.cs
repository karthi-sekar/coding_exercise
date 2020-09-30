using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AsyncCache.Contracts;
using AsyncCache.Extensions;
using AsyncCache.Helpers;
using AsyncCacheContract;
using AsyncCacheContract.Attribs;
using AsyncCacheContract.Enums;
using DotNetObjectsExt;

namespace AsyncCache.Lib.DbRelated
{
    internal sealed class DatabasePropertyMapper<TMap>
    {
        private readonly IConnectionMgr _connectionMgr;
        private readonly string _opname;
        private readonly CancellationToken _token;
        private readonly string _query;
        private readonly bool _isactualDataQuery;
        private readonly PropertySetterBuilder<TMap> _propertySetterBuilder;

        public DatabasePropertyMapper(IConnectionMgr connectionMgr, string opname, CancellationToken token, string query,
            bool isactualDataQuery)
        {
            _connectionMgr = connectionMgr;
            _opname = opname + "/validation";
            _token = token;
            _query = query;
            _isactualDataQuery = isactualDataQuery;
            _propertySetterBuilder = new PropertySetterBuilder<TMap>();
        }

        public async Task PrepareBinding()
        {
            var mapType = typeof(TMap);
            var userMapAttr =
                Attribute.GetCustomAttribute(mapType, typeof(AsyncCacheUserMapperAttribute), false) as
                    AsyncCacheUserMapperAttribute;

            var dbcolumnTypes = await DatabaseColumnTypeWithLowerCaseKey(_query).ConfigureAwait(false);
            QuerySetters = new Action<TMap, DbDataReader, int>[dbcolumnTypes.Count];

            if (userMapAttr == null)
            {
                PopulateBinding(dbcolumnTypes);
            }
            else
            {
                PopulateBinding(
                                _isactualDataQuery
                                    ? userMapAttr.UserMapForActualData
                                    : userMapAttr.UserMapForAdditionalData, dbcolumnTypes);
            }
        }

        private void PopulateBinding(string funcName, IReadOnlyDictionary<string, DatabaseSchema> dbcolumnTypes)
        {
            var userMapOfActualQuery = GetUserMapAndConvertKeyToLowerTrim(funcName);
            foreach (var key in dbcolumnTypes.Keys.Where(key => !userMapOfActualQuery.ContainsKey(key)))
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                    $"({_opname}) {typeof(TMap).Name} does not define delegate for db column (name:{key}) in the " +
                        $"static function {funcName}");
            }
            foreach (var key in userMapOfActualQuery.Keys.Where(key => !dbcolumnTypes.ContainsKey(key)))
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                    $"({_opname}) {typeof(TMap).Name} defines a delegate for db column (name:{key}) in the " +
                        $"static function {funcName} but this column does NOT exists in the query:" +
                        $"{Environment.NewLine}{_query}");
            }
            PopulateBindingWithMap(userMapOfActualQuery, dbcolumnTypes, QuerySetters);
        }

        private void PopulateBinding(IReadOnlyDictionary<string, DatabaseSchema> dbcolumnTypes)
        {
            PopulateBindingWithMap(new Dictionary<string, Delegate>(), dbcolumnTypes, QuerySetters);
        }

        private void PopulateBindingWithMap(IReadOnlyDictionary<string, Delegate> map,
            IReadOnlyDictionary<string, DatabaseSchema> dbcolumnTypes,
            IList<Action<TMap, DbDataReader, int>> querySetters)
        {
            var typeOfAttribute = typeof(AsyncCacheAttribute);
            var mapType = typeof(TMap);
            var propertyCount = 0;
            try
            {
                foreach (var prop in mapType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    var attr = Attribute.GetCustomAttribute(prop, typeOfAttribute, false) as AsyncCacheAttribute;
                    if (attr == null)
                    {
                        continue;
                    }
                    var dbcolumnName = _isactualDataQuery
                        ? attr.DbColumnName.SafeTrimLower()
                        : attr.AdditionalDataDbColumnName.SafeTrimLower();
                    dbcolumnName.ThrowIfNullOrWhiteSpace($"{nameof(AsyncCacheAttribute)}." +
                        (_isactualDataQuery ? $"{nameof(attr.DbColumnName)}" : $"{nameof(attr.AdditionalDataDbColumnName)}"));
                    prop.ThrowIfReadOnly(typeof(TMap).Name);
                    propertyCount++;

                    if (!_propertySetterBuilder.IsValidType(prop))
                    {
                        throw new InvalidOperationException(
                            $"{prop.Name} return type (value:{prop.PropertyType.Name}) is not known." +
                                $"Known types are:{_propertySetterBuilder.ValidTypes}");
                    }
                    Delegate propMapper;
                    if (!map.TryGetValue(dbcolumnName, out propMapper))
                    {
                        propMapper = _propertySetterBuilder.ToDelegate(prop);
                    }
                    DatabaseSchema schema;
                    if (!dbcolumnTypes.TryGetValue(dbcolumnName, out schema))
                    {
                        throw new InvalidOperationException(
                            $"{prop.Name} is associated to db column (value:{dbcolumnName}) which is not part of query");
                    }
                    querySetters[schema.ColumnPosition] = PrepareSetter(schema.ColumnType, prop.PropertyType,
                        attr.ConversionType, propMapper, dbcolumnName, prop.Name);
                }
            }
            catch (Exception e)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                    $"({_opname}) Error related to {typeof(TMap).Name}", e);
            }
            if (propertyCount == 0)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                    $"({_opname}) {typeof(TMap).Name} does not supply any db mapped properties " +
                        $"(decorated with {nameof(AsyncCacheAttribute)}).");
            }
            var nullIndex = querySetters.IndexOf(null);
            if (nullIndex < 0 || nullIndex >= querySetters.Count)
            {
                return;
            }
            var columnData = dbcolumnTypes.FirstOrDefault(x => x.Value.ColumnPosition == nullIndex);
            throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                $"({_opname}) {typeof(TMap).Name} does not supply any NON-public properties " +
                    $"(decorated with {nameof(AsyncCacheAttribute)}) for db column:{columnData.Key}.");
        }

        public Action<TMap, DbDataReader, int>[] QuerySetters { get; private set; }

        private IReadOnlyDictionary<string, Delegate> GetUserMapAndConvertKeyToLowerTrim(string staticMethodName)
        {
            var mapType = typeof(TMap);
            var staticMethod = mapType.GetMethod(staticMethodName, BindingFlags.Static | BindingFlags.NonPublic);
            if (staticMethod == null)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                    $"({_opname}) {typeof(TMap).Name} does not supply " +
                        $"{BindingFlags.Static | BindingFlags.NonPublic} method with name:{staticMethodName}");
            }
            var methodOutput = staticMethod.Invoke(null, null);
            if (methodOutput == null)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                    $"({_opname}) {typeof(TMap).Name} have void return type instead of Dictionary<string, Delegate>.");
            }
            var usermap = methodOutput as IReadOnlyDictionary<string, Delegate>;
            if (usermap == null)
            {
                throw new AsyncCacheException(AsyncCacheErrorCode.InvalidMap,
                    $"({_opname}) {typeof(TMap).Name} does not return " +
                        $"Dictionary<string, Delegate>. returned type:{methodOutput.GetType().FullName}");
            }

            return usermap.ToDictionary(x => x.Key.SafeTrimLower(), x => x.Value);
        }

        private Action<TMap, DbDataReader, int> PrepareSetter(Type columnType, Type propertyType,
            CacheStringConversion conversionType, Delegate setter, string columnName, string propName)
        {
            if (columnType == null)
            {
                return propertyType == typeof(string)
                    ? _propertySetterBuilder.StringSetter(conversionType, setter, columnName)
                    : _propertySetterBuilder.Setter(propertyType, setter, columnName);
            }
            return propertyType == typeof(string)
                ? _propertySetterBuilder.StringSetter(columnType, conversionType, setter, columnName)
                : _propertySetterBuilder.Setter(columnType, propertyType, setter, columnName, propName);
        }

        private async Task<IReadOnlyDictionary<string, DatabaseSchema>> DatabaseColumnTypeWithLowerCaseKey(string query)
        {
            var conn = await _connectionMgr.GetConnectionAsync(_opname, _token).ConfigureAwait(false);
            using (conn)
            {
                using (var comm = conn.GetCommand)
                {
                    comm.CommandText = query;
                    comm.CommandType = CommandType.Text;

                    var reader = await comm.ExecuteReaderAsync(CommandBehavior.SchemaOnly, _token).ConfigureAwait(false);
                    using (reader)
                    {
                        var result = new Dictionary<string, DatabaseSchema>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            result.Add(reader.GetName(i).SafeTrimLower(), new DatabaseSchema
                            {
                                ColumnType = reader.GetFieldType(i),
                                ColumnPosition = i
                            });
                        }
                        return result;
                    }
                }
            }
        }

        private sealed class DatabaseSchema
        {
            public int ColumnPosition { get; set; }
            public Type ColumnType { get; set; }
        }
    }
}