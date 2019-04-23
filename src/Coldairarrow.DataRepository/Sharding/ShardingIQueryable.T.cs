using Coldairarrow.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Coldairarrow.DataRepository
{
    public class ShardingQueryable<T> : IShardingQueryable<T>
    {
        public ShardingQueryable(IQueryable<T> source)
        {
            _source = source;
        }

        public Expression Expression => _source.Expression;

        public Type ElementType => _source.ElementType;

        public IQueryProvider Provider => _source.Provider;

        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        private IQueryable<T> _source { get; set; }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public List<T> ToList()
        {
            string tableName = (_source.GetObjQuery() as IQueryable).ElementType.Name;
            //去除分页,获取前Take+Skip数量
            int? take = _source.GetTakeCount();
            int? skip = _source.GetSkipCount();
            skip = skip == null ? 0 : skip;
            var (sortColumn, sortType) = _source.GetOrderBy();
            var noPaginSource = _source.RemoveTake().RemoveSkip();
            if (!take.IsNullOrEmpty())
                noPaginSource = noPaginSource.Take(take.Value + skip.Value);
            var (sql, parameters) = noPaginSource.ToSQL();

            //从各个分表获取数据
            var tables = ShardingConfig.Instance.GetReadTables(tableName);
            List<Task<List<T>>> tasks = new List<Task<List<T>>>();
            tables.ForEach(aTable =>
            {
                tasks.Add(Task.Run(() =>
               {
                   var newSql = BuildNewSql(tableName, sql, parameters, aTable.tableName, aTable.dbType);
                   return DbHelperFactory.GetDbHelper(aTable.dbType, aTable.conString).GetListBySql<T>(newSql.newSql, newSql.newParamters);
               }));
            });
            Task.WaitAll(tasks.ToArray());
            List<T> all = new List<T>();
            tasks.ForEach(aTask =>
            {
                all.AddRange(aTask.Result);
            });

            //合并数据
            var resList = all;
            if (!sortColumn.IsNullOrEmpty() && !sortType.IsNullOrEmpty())
                resList = all.OrderBy($"{sortColumn} {sortType}").ToList();
            if (!skip.IsNullOrEmpty())
                resList = all.Skip(skip.Value).ToList();
            if (!take.IsNullOrEmpty())
                resList = all.Skip(take.Value).ToList();

            return resList;

            (string newSql, List<DbParameter> newParamters) BuildNewSql(string oldTableName, string oldSql, List<ObjectParameter> oldParamters, string newtableName, DatabaseType dbType)
            {
                string newSql = oldSql.Replace(oldTableName, newtableName);
                var newParamters = oldParamters.Select(x =>
                {
                    var theParamter = DbProviderFactoryHelper.GetDbParameter(dbType);
                    theParamter.ParameterName = x.Name;
                    theParamter.Value = x.Value;

                    return theParamter;
                }).ToList();

                return (newSql, newParamters);
            }
        }

        public IShardingQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            _source = _source.Where(predicate);

            return this;
        }

        public IShardingQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _source = _source.OrderBy(keySelector);

            return this;
        }

        public IShardingQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _source = _source.OrderByDescending(keySelector);

            return this;
        }

        public IShardingQueryable<T> Skip(int count)
        {
            _source = _source.Skip(count);

            return this;
        }

        public IShardingQueryable<T> Take(int count)
        {
            _source = _source.Take(count);

            return this;
        }
    }
}
