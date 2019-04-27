using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
            _absTableType = (_source.GetObjQuery() as IQueryable).ElementType;
            _absTableName = _absTableType.Name;
        }
        private Type _absTableType { get; }
        private string _absTableName { get; }
        private IQueryable<T> _source { get; set; }
        private Type MapTable(Type absTable, string targetTableName)
        {
            var config = TypeBuilderHelper.GetConfig(absTable);
            config.Attributes.RemoveAll(x => x.Attribute == typeof(TableAttribute));
            config.FullName = $"Coldairarrow.DataRepository.{targetTableName}";

            return TypeBuilderHelper.BuildType(config);
        }
        public List<T> ToList()
        {
            //去除分页,获取前Take+Skip数量
            int? take = _source.GetTakeCount();
            int? skip = _source.GetSkipCount();
            skip = skip == null ? 0 : skip;
            var (sortColumn, sortType) = _source.GetOrderBy();
            var noPaginSource = _source.RemoveTake().RemoveSkip();
            if (!take.IsNullOrEmpty())
                noPaginSource = noPaginSource.Take(take.Value + skip.Value);

            //从各个分表获取数据
            var tables = ShardingConfig.Instance.GetReadTables(_absTableName);
            List<Task<List<T>>> tasks = new List<Task<List<T>>>();
            tables.ForEach(aTable =>
            {
                tasks.Add(Task.Run(() =>
                {
                    var targetTable = MapTable(_absTableType, aTable.tableName);
                    var targetIQ = DbFactory.GetRepository(aTable.conString, aTable.dbType).GetIQueryable(targetTable);
                    var newQ = _source.ChangeSource(targetIQ);
                    var list = newQ
                        .CastToList<object>()
                        .Select(x => x.ChangeType<T>())
                        .ToList();

                    return list;
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
        public List<T> GetPagination(Pagination pagination)
        {
            pagination.records = Count();
            _source = _source.OrderBy($"{pagination.sidx} {pagination.sord}");

            return Skip((pagination.page - 1) * pagination.rows).Take(pagination.rows).ToList();
        }
        private List<object> GetStatisData(Func<IQueryable, object> access)
        {
            var tables = ShardingConfig.Instance.GetReadTables(_absTableName);
            List<Task<object>> tasks = new List<Task<object>>();
            tables.ForEach(aTable =>
            {
                tasks.Add(Task.Run(() =>
                {
                    var targetTable = MapTable(_absTableType, aTable.tableName);
                    var targetIQ = DbFactory.GetRepository(aTable.conString, aTable.dbType).GetIQueryable(targetTable);
                    var newQ = _source.ChangeSource(targetIQ);

                    return access(newQ);
                }));
            });
            Task.WaitAll(tasks.ToArray());

            return tasks.Select(x => x.Result).ToList();
        }
        public int Count()
        {
            return GetStatisData(x => x.Count()).Sum(x => (int)x);
        }
        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            return GetStatisData(x => x.Max(selector)).Max(x => (TResult)x);
        }
        public TResult Min<TResult>(Expression<Func<T, TResult>> selector)
        {
            return GetStatisData(x => x.Min(selector)).Min(x => (TResult)x);
        }
        private dynamic DynamicSum(List<dynamic> datas)
        {
            dynamic sum = 0;
            datas.ForEach(aData =>
            {
                sum += aData;
            });

            return sum;
        }
        private dynamic DynamicAverage(dynamic selector)
        {
            var list = GetStatisData(x => new KeyValuePair<dynamic, dynamic>(x.Count(), Coldairarrow.Util.Extention.Sum(x, selector))).Select(x => (KeyValuePair<dynamic, dynamic>)x).ToList();
            var count = DynamicSum(list.Select(x => x.Key).ToList());
            var sum = DynamicSum(list.Select(x => x.Value).ToList());

            return (sum / count);
        }
        public double Average(Expression<Func<T, int>> selector)
        {
            return (double)DynamicAverage(selector);
        }
        public double? Average(Expression<Func<T, int?>> selector)
        {
            return (double?)DynamicAverage(selector);
        }
        public float Average(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }
        public float? Average(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Average(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Average(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal Average(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Sum(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }
        public float Sum(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }
        public int Sum(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }
        public long Sum(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }
    }
}
