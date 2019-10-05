﻿using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Coldairarrow.Util
{
    /// <summary>
    /// SqlServer数据库操作帮助类
    /// </summary>
    public class OracleHelper : DbHelper
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nameOrConStr">数据库连接名或连接字符串</param>
        public OracleHelper(string nameOrConStr)
            : base(DatabaseType.Oracle, nameOrConStr)
        {
        }

        #endregion

        #region 私有成员

        protected override Dictionary<string, Type> DbTypeDic { get; } = new Dictionary<string, Type>()
        {
            { "BFILE", typeof(byte[]) },
            { "BLOB", typeof(byte[]) },
            { "CHAR", typeof(string) },
            { "CLOB", typeof(string) },
            { "DATE", typeof(DateTime) },
            { "FLOAT", typeof(decimal) },
            { "INTEGER", typeof(decimal) },
            { "INTERVAL YEAR TO MONTH", typeof(Int32) },
            { "INTERVAL DAY TO SECOND", typeof(TimeSpan) },
            { "LONG", typeof(string) },
            { "LONG RAW", typeof(string[]) },
            { "NCHAR", typeof(string) },
            { "NCLOB", typeof(string) },
            { "NUMBER", typeof(decimal) },
            { "NVARCHAR2", typeof(string) },
            { "RAW", typeof(byte[]) },
            { "ROWID", typeof(string) },
            { "TIMESTAMP", typeof(DateTime) },
            { "TIMESTAMP WITH LOCAL TIME ZONE", typeof(DateTime) },
            { "TIMESTAMP WITH TIME ZONE", typeof(DateTime) },
            { "UNSIGNED INTEGER", typeof(decimal) },
            { "VARCHAR2", typeof(string) }
        };

        #endregion

        #region 外部接口

        /// <summary>
        /// 获取数据库中的所有表
        /// </summary>
        /// <param name="schemaName">模式（架构）</param>
        /// <returns></returns>
        public override List<DbTableInfo> GetDbAllTables(string schemaName = null)
        {
            string sql = @"SELECT A.TABLE_NAME AS TABLENAME, B.comments AS DESCRIPTION
  FROM USER_TABLES A, USER_TAB_COMMENTS B
 WHERE A.table_name = B.table_name(+)";
            return GetListBySql<DbTableInfo>(sql);
        }

        /// <summary>
        /// 通过连接字符串和表名获取数据库表的信息
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public override List<TableInfo> GetDbTableInfo(string tableName)
        {
            string sql = @"
SELECT A.COLUMN_NAME AS NAME,
       A.DATA_TYPE   AS TYPE,
       NVL2(D.CONSTRAINT_TYPE,1,0) AS ISKEY,
       DECODE(A.NULLABLE,'Y',1,0) AS ISNULLABLE,
       B.COMMENTS AS DESCRIPTION
  FROM USER_TAB_COLUMNS A, USER_COL_COMMENTS B, USER_IND_COLUMNS C, USER_CONSTRAINTS D
 WHERE A.TABLE_NAME = B.TABLE_NAME(+)
   AND A.COLUMN_NAME = B.COLUMN_NAME(+)
   AND A.TABLE_NAME = C.TABLE_NAME(+)
   AND A.COLUMN_NAME = C.COLUMN_NAME(+)
   AND C.INDEX_NAME = D.INDEX_NAME(+)
   AND 'P' = D.CONSTRAINT_TYPE(+)
   AND A.TABLE_NAME= :tableName
 ORDER BY A.COLUMN_ID";
            return GetListBySql<TableInfo>(sql, new List<DbParameter> { new OracleParameter(":table_name", tableName) });
        }

        /// <summary>
        /// 生成实体文件
        /// </summary>
        /// <param name="infos">表字段信息</param>
        /// <param name="tableName">表名</param>
        /// <param name="tableDescription">表描述信息</param>
        /// <param name="filePath">文件路径（包含文件名）</param>
        /// <param name="nameSpace">实体命名空间</param>
        /// <param name="schemaName">架构（模式）名</param>
        public override void SaveEntityToFile(List<TableInfo> infos, string tableName, string tableDescription, string filePath, string nameSpace, string schemaName = null)
        {
            base.SaveEntityToFile(infos, tableName, tableDescription, filePath, nameSpace, schemaName);
        }

        #endregion
    }
}
