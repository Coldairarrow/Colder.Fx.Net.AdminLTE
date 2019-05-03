using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public class ShardingRepository : IShardingRepository
    {
        #region 构造函数

        public ShardingRepository(IRepository db)
        {
            _db = db;
        }

        #endregion

        #region 私有成员

        private IRepository _db { get; }

        private Type MapTable(Type absTable, string targetTableName)
        {
            return ShardingHelper.MapTable(absTable, targetTableName);
        }

        private List<(object targetObj, IRepository targetDb)> GetMapConfigs<T>(List<T> entities)
        {
            List<(object targetObj, IRepository targetDb)> resList = new List<(object targetObj, IRepository targetDb)>();
            entities.ForEach(aEntity =>
            {
                (string tableName, string conString, DatabaseType dbType) = ShardingConfig.Instance.GetTheWriteTable(typeof(T).Name, aEntity);

                resList.Add((aEntity.ChangeType(MapTable(typeof(T), tableName)), DbFactory.GetRepository(conString, dbType)));
            });

            return resList;
        }

        private void WriteTable<T>(List<T> entities, Action<object, IRepository> accessData)
        {
            var mapConfigs = GetMapConfigs(entities);
            DistributedTransaction transaction = new DistributedTransaction(mapConfigs.Select(x => x.targetDb).ToArray());
            transaction.BeginTransaction();
            mapConfigs.ForEach(aConfig =>
            {
                accessData(aConfig.targetObj, aConfig.targetDb);
            });
            transaction.EndTransaction();
        }

        #endregion

        #region 外部接口

        public void Insert<T>(T entity) where T : class, new()
        {
            Insert(new List<T> { entity });
        }

        public void Insert<T>(List<T> entities) where T : class, new()
        {
            WriteTable(entities, (targetObj, targetDb) => targetDb.Insert(targetObj));
        }

        public void Delete<T>(T entity) where T : class, new()
        {
            Delete(new List<T> { entity });
        }

        public void Delete<T>(List<T> entities) where T : class, new()
        {
            WriteTable(entities, (targetObj, targetDb) => targetDb.Delete(targetObj));
        }

        public void Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            var deleteList = GetIShardingQueryable<T>().Where(condition).ToList();

            Delete(deleteList);
        }

        public void Update<T>(T entity) where T : class, new()
        {
            Update(new List<T> { entity });
        }

        public void Update<T>(List<T> entities) where T : class, new()
        {
            WriteTable(entities, (targetObj, targetDb) => targetDb.Update(targetObj));
        }

        public void UpdateAny<T>(T entity, List<string> properties) where T : class, new()
        {
            UpdateAny(new List<T> { entity }, properties);
        }

        public void UpdateAny<T>(List<T> entities, List<string> properties) where T : class, new()
        {
            WriteTable(entities, (targetObj, targetDb) => targetDb.UpdateAny(targetObj, properties));
        }

        public void UpdateWhere<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new()
        {
            var list = GetIShardingQueryable<T>().Where(whereExpre).ToList();
            list.ForEach(aData => set(aData));
            Update(list);
        }

        public IShardingQueryable<T> GetIShardingQueryable<T>() where T : class, new()
        {
            return new ShardingQueryable<T>(_db.GetIQueryable<T>());
        }

        public List<T> GetList<T>() where T : class, new()
        {
            return GetIShardingQueryable<T>().ToList();
        }

        #endregion
    }
}
