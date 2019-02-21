using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public class OracleRepository : DbRepository, IRepository
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public OracleRepository()
            : base(null, DatabaseType.Oracle, null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conStr">数据库连接名</param>
        public OracleRepository(string conStr)
            : base(conStr, DatabaseType.Oracle, null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conStr">数据库连接名</param>
        /// <param name="entityNamespace">实体命名空间</param>
        public OracleRepository(string conStr, string entityNamespace)
            : base(conStr, DatabaseType.Oracle, entityNamespace)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">数据库连接上下文</param>
        public OracleRepository(DbContext dbContext)
            : base(dbContext, DatabaseType.Oracle, null)
        {
        }

        #endregion

        #region 插入数据

        /// <summary>
        /// 使用Bulk批量插入数据（适合大数据量，速度非常快）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">数据</param>
        public override void BulkInsert<T>(List<T> entities)
        {
            throw new Exception("抱歉！暂不支持Oracle！");
        }

        public override void Delete_Sql<T>(Expression<Func<T, bool>> condition)
        {
            Delete(condition);
        }

        #endregion
    }
}
