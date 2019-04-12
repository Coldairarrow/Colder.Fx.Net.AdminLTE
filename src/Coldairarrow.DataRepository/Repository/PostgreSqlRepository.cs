using Coldairarrow.Util;
using System;
using System.Collections.Generic;

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

        public override void BulkInsert<T>(List<T> entities)
        {
            throw new Exception("抱歉！暂不支持PostgreSql！");
        }

        public override void DeleteAll(Type type)
        {
            string tableName = GetDbTableName(type);
            string sql = $"DELETE FROM \"{tableName}\"";
            ExecuteSql(sql);
        }

        #endregion
    }
}
