using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public class PostgreSqlRepository : DbRepository, IRepository
    {
        #region 构造函数

        public PostgreSqlRepository()
            : base(null, DatabaseType.PostgreSql, null)
        {
        }

        public PostgreSqlRepository(string conStr)
            : base(conStr, DatabaseType.PostgreSql, null)
        {
        }

        public PostgreSqlRepository(string conStr, string entityNamespace)
            : base(conStr, DatabaseType.PostgreSql, entityNamespace)
        {
        }

        #endregion

        #region 插入数据

        /// <summary>
        /// 使用Bulk批量导入,速度快
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="entities">实体集合</param>
        /// <exception cref="Exception">抱歉！暂不支持PostgreSql！</exception>
        public override void BulkInsert<T>(List<T> entities)
        {
            throw new Exception("抱歉！暂不支持PostgreSql！");
        }

        /// <summary>
        /// 删除所有记录
        /// </summary>
        /// <param name="type">实体类型</param>
        public override void DeleteAll(Type type)
        {
            string tableName = GetDbTableName(type);
            string sql = $"DELETE FROM \"{tableName}\"";
            ExecuteSql(sql);
        }

        #endregion
    }
}
