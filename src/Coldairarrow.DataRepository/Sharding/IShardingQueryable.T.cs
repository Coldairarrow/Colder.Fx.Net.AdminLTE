using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public interface IShardingQueryable<T> : IQueryable<T>
    {
        List<T> ToList();
        IShardingQueryable<T> Where(Expression<Func<T, bool>> predicate);
        int Count();
        IShardingQueryable<T> Skip(int count);
        IShardingQueryable<T> Take(int count);
        IShardingQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
        IShardingQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
    }
}
