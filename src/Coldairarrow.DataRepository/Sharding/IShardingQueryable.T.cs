using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public interface IShardingQueryable<T> : IQueryable<T>
    {
        List<T> ToList();
        IShardingQueryable<T> Where<TSource>(Expression<Func<TSource, bool>> predicate);
        int Count();
        IShardingQueryable<T> OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector);
        IShardingQueryable<T> OrderByDescending<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector);
    }
}
