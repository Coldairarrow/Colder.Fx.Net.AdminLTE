using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public class ShardingRepository : IShardingRepository
    {
        public ShardingRepository(IRepository db)
        {
            _db = db;
        }

        private IRepository _db { get; }
        public void Delete<T>(string key) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(List<string> keys) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(T entity) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(List<T> entities) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Delete_Sql<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public T GetEntity<T>(params object[] keyValue) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public IShardingQueryable<T> GetIShardingQueryable<T>() where T : class, new()
        {
            return new ShardingQueryable<T>(_db.GetIQueryable<T>());
        }

        public List<T> GetList<T>() where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(T entity) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Insert<T>(List<T> entities) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Update<T>(T entity) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void Update<T>(List<T> entities) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void UpdateAny<T>(T entity, List<string> properties) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void UpdateAny<T>(List<T> entities, List<string> properties) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public void UpdateWhere<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new()
        {
            throw new NotImplementedException();
        }
    }
}
