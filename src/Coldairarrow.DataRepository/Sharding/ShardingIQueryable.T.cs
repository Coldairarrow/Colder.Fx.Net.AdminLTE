using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        private IQueryable<T> _source { get; }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public List<T> ToList()
        {
            throw new NotImplementedException();
        }

        public IShardingQueryable<T> Where<TSource>(Expression<Func<TSource, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IShardingQueryable<T> OrderBy<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public IShardingQueryable<T> OrderByDescending<TSource, TKey>(Expression<Func<TSource, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }
    }
}
