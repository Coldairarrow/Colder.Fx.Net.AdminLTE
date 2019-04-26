using Coldairarrow.Util;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Coldairarrow.DataRepository
{
    public class RepositoryDbContext : IRepositoryDbContext
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conString">数据库连接名或连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">数据库实体命名空间,注意,该命名空间应该包含所有需要的数据库实体</param>
        public RepositoryDbContext(string conString, DatabaseType dbType, string entityNamespace)
        {
            _conString = conString;
            _dbType = dbType;
            RefreshDb();
        }

        #endregion

        #region 外部接口

        public Database Database => _db.Database;

        public DbEntityEntry Entry(object entity)
        {
            object targetObj = null;
            var type = entity.GetType();
            var model = CheckModel(entity.GetType());
            if (type == model)
                targetObj = entity;
            else
                targetObj = entity.ChangeType(model);

            return _db.Entry(targetObj);
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            return Entry(entity as object).Cast<TEntity>();
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _db.Set(typeof(TEntity)).Cast<TEntity>();
        }

        public DbSet Set(Type entityType)
        {
            CheckModel(entityType);

            return _db.Set(entityType);
        }

        public int SaveChanges()
        {
            return _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public DbContext GetDbContext()
        {
            return _db;
        }

        #endregion

        #region 私有成员

        private DbContext _db { get; set; }
        private DatabaseType _dbType { get; }
        private string _conString { get; }
        private void RefreshDb()
        {
            var oldDb = _db;
            var con = DbProviderFactoryHelper.GetDbConnection(_conString, _dbType);
            var dBCompiledModel = DbModelFactory.GetDbCompiledModel(_conString, _dbType);
            _db = new BaseDbContext(con, dBCompiledModel);
            if (oldDb != null)
                _db.Database.Log += oldDb.Database.Log;
        }
        private Type CheckModel(Type type)
        {
            (bool needRefresh, Type model) model = DbModelFactory.GetModel(type);
            if (model.needRefresh)
                RefreshDb();

            return model.model;
        }

        #endregion
    }
}
