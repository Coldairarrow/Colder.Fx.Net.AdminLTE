using Coldairarrow.Util;
using System;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public class DbFactory
    {
        #region 外部接口

        /// <summary>
        /// 根据配置文件获取数据库类型，并返回对应的工厂接口
        /// </summary>
        /// <param name="conString">链接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">实体命名空间</param>
        /// <returns></returns>
        public static IRepository GetRepository(string conString = null, DatabaseType? dbType = null, string entityNamespace = null)
        {
            conString = conString.IsNullOrEmpty() ? GlobalSwitch.DefaultDbConName : conString;
            conString = DbProviderFactoryHelper.GetConStr(conString);
            dbType = dbType.IsNullOrEmpty() ? GlobalSwitch.DatabaseType : dbType;
            entityNamespace = entityNamespace.IsNullOrEmpty() ? GlobalSwitch.DefaultEntityNamespace : entityNamespace;
            Type dbRepositoryType = Type.GetType("Coldairarrow.DataRepository." + DbProviderFactoryHelper.DbTypeToDbTypeStr(dbType.Value) + "Repository");

            return Activator.CreateInstance(dbRepositoryType, new object[] { conString, entityNamespace }) as IRepository;
        }

        /// <summary>
        /// 获取ShardingRepository
        /// </summary>
        /// <returns></returns>
        public static IShardingRepository GetShardingRepository()
        {
            return new ShardingRepository(GetRepository());
        }

        /// <summary>
        /// 根据参数获取数据库的DbContext
        /// </summary>
        /// <param name="conString">初始化参数，可为连接字符串或者DbContext</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">实体命名空间</param>
        /// <returns></returns>
        public static IRepositoryDbContext GetDbContext(string conString, DatabaseType dbType, string entityNamespace)
        {
            IRepositoryDbContext dbContext = new RepositoryDbContext(conString, dbType, entityNamespace);
            dbContext.Database.CommandTimeout = 5 * 60;

            return dbContext;
        }

        #endregion
    }
}
