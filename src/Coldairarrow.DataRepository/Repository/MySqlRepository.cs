using Coldairarrow.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Coldairarrow.DataRepository
{
    public class MySqlRepository : DbRepository, IRepository
    {
        #region 构造函数

        public MySqlRepository()
            : base(null, DatabaseType.MySql, null)
        {
        }

        public MySqlRepository(string conStr)
            : base(conStr, DatabaseType.MySql, null)
        {
        }

        public MySqlRepository(string conStr, string entityNamespace)
            : base(conStr, DatabaseType.MySql, entityNamespace)
        {
        }

        #endregion

        #region 特殊操作

        /// <summary>
        /// 使用Bulk批量导入,速度快
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="entities">实体集合</param>
        public override void BulkInsert<T>(List<T> entities)
        {
            DataTable dt = entities.ToDataTable();
            using (MySqlConnection conn = new MySqlConnection())
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

                int insertCount = 0;
                string tmpPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString() + "_" + Guid.NewGuid().ToString() + ".tmp");
                string csv = dt.ToCsvStr();
                File.WriteAllText(tmpPath, csv, Encoding.UTF8);

                using (MySqlTransaction tran = conn.BeginTransaction())
                {
                    MySqlBulkLoader bulk = new MySqlBulkLoader(conn)
                    {
                        FieldTerminator = ",",
                        FieldQuotationCharacter = '"',
                        EscapeCharacter = '"',
                        LineTerminator = "\r\n",
                        FileName = tmpPath,
                        NumberOfLinesToSkip = 0,
                        TableName = tableName,
                    };
                    try
                    {
                        bulk.Columns.AddRange(dt.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToList());
                        insertCount = bulk.Load();
                        tran.Commit();
                    }
                    catch (MySqlException ex)
                    {
                        if (tran != null)
                            tran.Rollback();

                        throw ex;
                    }
                }
                File.Delete(tmpPath);
            }
        }

        #endregion
    }
}
