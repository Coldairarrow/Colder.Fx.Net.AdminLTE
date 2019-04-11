using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Coldairarrow.DataRepository
{
    public class SqlServerRepository : DbRepository, IRepository
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public SqlServerRepository()
            : base(null, DatabaseType.SqlServer, null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conStr">数据库连接名</param>
        public SqlServerRepository(string conStr)
            : base(conStr, DatabaseType.SqlServer, null)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conStr">数据库连接名</param>
        /// <param name="entityNamespace">实体命名空间</param>
        public SqlServerRepository(string conStr, string entityNamespace)
            : base(conStr, DatabaseType.SqlServer, entityNamespace)
        {
        }

        #endregion

        #region 特殊方法

        public override void BulkInsert<T>(List<T> entities)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = _conString;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                string tableName = string.Empty;
                var tableAttribute = typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
                if (tableAttribute != null)
                    tableName = ((TableAttribute)tableAttribute).Name;
                else
                    tableName = typeof(T).Name;

                SqlBulkCopy sqlBC = new SqlBulkCopy(conn)
                {
                    BatchSize = 100000,
                    BulkCopyTimeout = 0,
                    DestinationTableName = tableName
                };
                using (sqlBC)
                {
                    sqlBC.WriteToServer(entities.ToDataTable());
                }
            }
        }

        public override void Delete_Sql<T>(Expression<Func<T, bool>> condition)
        {
            var objectQuery = GetObjectQueryFromDbQueryable(GetIQueryable<T>().Where(condition));
            string querySTr = objectQuery.ToTraceString();
            string parttern = "^SELECT.*?FROM.*?AS(.*?)WHERE.*?$";
            var match = Regex.Match(querySTr, parttern, RegexOptions.Singleline);
            string extent1 = match.Groups[1].ToString();

            parttern = "^SELECT.*?(FROM.*?AS.*?WHERE.*?$)";
            match = Regex.Match(querySTr, parttern, RegexOptions.Singleline);
            string fromSql = match.Groups[1].ToString();

            string deleteSql = $"DELETE {extent1} {fromSql}";
            List<DbParameter> dbParamters = new List<DbParameter>();

            objectQuery.Parameters.ToList().ForEach(aParamter =>
            {
                var parameter = DbProviderFactoryHelper.GetDbParameter(_dbType);
                parameter.ParameterName = aParamter.Name;
                parameter.Value = aParamter.Value ?? DBNull.Value;

                dbParamters.Add(parameter);
            });

            ExecuteSql(deleteSql, dbParamters);
        }

        #endregion
    }
}
