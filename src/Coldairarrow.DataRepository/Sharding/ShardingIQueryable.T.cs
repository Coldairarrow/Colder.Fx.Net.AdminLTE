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

        private IQueryable<T> _source { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}
