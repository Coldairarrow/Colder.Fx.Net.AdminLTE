using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public class DbFactory
    {
        #region 构造函数

        static DbFactory()
        {
            _dbrepositoryContainer = new IocHelper();
            _dbrepositoryContainer.RegisterType<IRepository, SqlServerRepository>(DatabaseType.SqlServer.ToString());
            _dbrepositoryContainer.RegisterType<IRepository, MySqlRepository>(DatabaseType.MySql.ToString());
            _dbrepositoryContainer.RegisterType<IRepository, PostgreSqlRepository>(DatabaseType.PostgreSql.ToString());
            _dbrepositoryContainer.RegisterType<IRepository, OracleRepository>(DatabaseType.Oracle.ToString());
        }

        #endregion

        #region 内部成员

        private static IocHelper _dbrepositoryContainer { get; }

        #endregion

        #region 外部接口

        /// <summary>
        /// 根据配置文件获取数据库类型，并返回对应的工厂接口
        /// </summary>
        /// <param name="obj">初始化参数，可为连接字符串或者DbContext</param>
        /// <returns></returns>
        public static IRepository GetRepository(Object obj = null, DatabaseType? dbType = null, string entityNamespace = null)
        {
            IRepository res = null;
            DatabaseType _dbType = GetDbType(dbType);
            Type dbRepositoryType = Type.GetType("Coldairarrow.DataRepository." + DbProviderFactoryHelper.DbTypeToDbTypeStr(_dbType) + "Repository");
            if (obj.IsNullOrEmpty())
                obj = GlobalSwitch.DefaultDbConName;
            res = _dbrepositoryContainer.Resolve<IRepository>(_dbType.ToString(), obj, entityNamespace);

            return res;
        }

        /// <summary>
        /// 获取DbType
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        private static DatabaseType GetDbType(DatabaseType? dbType)
        {
            DatabaseType _dbType;
            if (dbType.IsNullOrEmpty())
            {
                _dbType = GlobalSwitch.DatabaseType;
            }
            else
                _dbType = dbType.Value;

            return _dbType;
        }

        /// <summary>
        /// 根据参数获取数据库的DbContext
        /// </summary>
        /// <param name="obj">初始化参数，可为连接字符串或者DbContext</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        public static IRepositoryDbContext GetDbContext(object obj, DatabaseType dbType, string entityNamespace)
        {
            IRepositoryDbContext dbContext = null;

            if (obj.IsNullOrEmpty())
            {
                dbContext = new RepositoryDbContext(null, dbType, entityNamespace);
            }
            else
            {
                //若参数为字符串
                if (obj is string)
                    dbContext = new RepositoryDbContext((string)obj, dbType, entityNamespace);
                else
                    throw new Exception("请传入有效的参数！");
            }

            dbContext.Database.CommandTimeout = 5 * 60;
            return dbContext;
        }

        #endregion
    }
}
