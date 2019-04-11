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
        /// 静态构造函数
        /// </summary>
        static RepositoryDbContext()
        {
            //数据库已手动构建，不需要自己生成初始化
            Database.SetInitializer<BaseDbContext>(null);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nameOrConStr">数据库连接名或连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">数据库实体命名空间,注意,该命名空间应该包含所有需要的数据库实体</param>
        public RepositoryDbContext(string nameOrConStr, DatabaseType dbType, string entityNamespace)
        {
            _dbConStr = DbProviderFactoryHelper.GetConStr(nameOrConStr);
            _dbType = dbType;
            RefreshDb();
        }

        #endregion

        #region 外部接口

        public Database Database => _db.Database;

        public DbEntityEntry Entry(object entity)
        {
            CheckModel(entity.GetType());

            return _db.Entry(entity);
        }

        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            CheckModel(entity.GetType());

            return _db.Entry(entity);
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            CheckModel(typeof(TEntity));

            return _db.Set<TEntity>();
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
        private string _dbConStr { get; }
        private void RefreshDb()
        {
            var con = DbProviderFactoryHelper.GetDbConnection(_dbConStr, _dbType);
            var dBCompiledModel = DbModelFactory.GetDbCompiledModel(_dbConStr, _dbType);
            _db = new BaseDbContext(con, dBCompiledModel);
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
