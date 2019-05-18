using Coldairarrow.Util;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// DbContext容器
    /// </summary>
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
            var targetModel = CheckModel(entityType);

            return _db.Set(targetModel);
        }

        public int SaveChanges()
        {
            return _db.SaveChanges();
        }

        public DbContext GetDbContext()
        {
            return _db;
        }

        public Type CheckEntityType(Type entityType)
        {
            return CheckModel(entityType);
        }
        private DbTransaction _transaction { get; set; }

        public void UseTransaction(DbTransaction transaction)
        {
            _transaction = transaction;
        }

        #endregion

        #region 私有成员

        private DbContext _db { get; set; }
        private DatabaseType _dbType { get; }
        private string _conString { get; }
        public Action<string> HandleSqlLog { get; set; }
        private void RefreshDb()
        {
            //重用DbConnection,使用底层相同的DbConnection,支持Model持热更新
            DbConnection con = _db == null ? DbProviderFactoryHelper.GetDbConnection(_conString, _dbType) : _db.Database.Connection;
            var dBCompiledModel = DbModelFactory.GetDbCompiledModel(_conString, _dbType);
            _db = new BaseDbContext(con, dBCompiledModel);
            _db.Database.UseTransaction(_transaction);
            _db.Database.Log = HandleSqlLog;
            disposedValue = false;
        }
        private Type CheckModel(Type type)
        {
            (bool needRefresh, Type model) model = DbModelFactory.GetModel(type);
            if (model.needRefresh)
                RefreshDb();

            return model.model;
        }

        #endregion

        #region Dispose

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }

                disposedValue = true;
            }
        }

        ~RepositoryDbContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
