using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Coldairarrow.Util
{
    /// <summary>
    /// SqlServer数据库操作帮助类
    /// </summary>
    public class SqlServerHelper : DbHelper
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nameOrConStr">数据库连接名或连接字符串</param>
        public SqlServerHelper(string nameOrConStr)
            : base(DatabaseType.SqlServer, nameOrConStr)
        {
        }

        #endregion

        #region 私有成员

        protected override Dictionary<string, Type> DbTypeDic { get; } = new Dictionary<string, Type>()
        {
            { "int", typeof(Int32) },
            { "text", typeof(string) },
            { "bigint", typeof(Int64) },
            { "binary", typeof(byte[]) },
            { "bit", typeof(bool) },
            { "char", typeof(string) },
            { "date", typeof(DateTime) },
            { "datetime", typeof(DateTime) },
            { "datetime2", typeof(DateTime) },
            { "decimal", typeof(decimal) },
            { "float", typeof(double) },
            { "image", typeof(byte[]) },
            { "money", typeof(decimal) },
            { "nchar", typeof(string) },
            { "ntext", typeof(string) },
            { "numeric", typeof(decimal) },
            { "nvarchar", typeof(string) },
            { "real", typeof(Single) },
            { "smalldatetime", typeof(DateTime) },
            { "smallint", typeof(Int16) },
            { "smallmoney", typeof(decimal) },
            { "timestamp", typeof(DateTime) },
            { "tinyint", typeof(byte) },
            { "varbinary", typeof(byte[]) },
            { "varchar", typeof(string) },
            { "variant", typeof(object) },
            { "uniqueidentifier", typeof(Guid) },
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
            if (schemaName.IsNullOrEmpty())
                schemaName = "dbo";

            string sql = @"select
[TableName] = a.name,
[Description] = g.value
from
  sys.tables a left join sys.extended_properties g
  on (a.object_id = g.major_id AND g.minor_id = 0 AND g.name= 'MS_Description')
UNION
select
[TableName] = a.name,
[Description] = g.value
from
  sys.views a left join sys.extended_properties g
  on (a.object_id = g.major_id AND g.minor_id = 0 AND g.name= 'MS_Description')";
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
select 
sys.columns.column_id as [ColumnId],
sys.columns.name as [Name],
sys.types.name as [Type], 
sys.columns.is_nullable [IsNullable], 
[IsIdentity]=CONVERT(BIT, (select count(*) from sys.identity_columns where sys.identity_columns.object_id = sys.columns.object_id and sys.columns.column_id = sys.identity_columns.column_id)),
  (select value from sys.extended_properties where sys.extended_properties.major_id = sys.columns.object_id and sys.extended_properties.minor_id = sys.columns.column_id and name='MS_Description') as [Description],
  [IsKey] =CONVERT(bit,(case when sys.columns.name in (select b.column_name
from information_schema.table_constraints a
inner join information_schema.constraint_column_usage b
on a.constraint_name = b.constraint_name
where a.constraint_type = 'PRIMARY KEY' and a.table_name = @table_name) then 1 else 0 end))
  from sys.columns, sys.views, sys.types where sys.columns.object_id = sys.views.object_id and sys.columns.system_type_id=sys.types.system_type_id and sys.views.name=@table_name and sys.types.name !='sysname' 

union

select
sys.columns.column_id as [ColumnId],
sys.columns.name as [Name],
sys.types.name as [Type], 
sys.columns.is_nullable [IsNullable], 
[IsIdentity]=CONVERT(BIT, (select count(*) from sys.identity_columns where sys.identity_columns.object_id = sys.columns.object_id and sys.columns.column_id = sys.identity_columns.column_id)),
  (select value from sys.extended_properties where sys.extended_properties.major_id = sys.columns.object_id and sys.extended_properties.minor_id = sys.columns.column_id and name='MS_Description') as [Description],
  [IsKey] =CONVERT(bit,(case when sys.columns.name in (select b.column_name
from information_schema.table_constraints a
inner join information_schema.constraint_column_usage b
on a.constraint_name = b.constraint_name
where a.constraint_type = 'PRIMARY KEY' and a.table_name = @table_name) then 1 else 0 end))
  from sys.columns, sys.tables, sys.types where sys.columns.object_id = sys.tables.object_id and sys.columns.system_type_id=sys.types.system_type_id and sys.tables.name=@table_name and sys.types.name !='sysname' 
order by sys.columns.column_id asc";
            return GetListBySql<TableInfo>(sql, new List<DbParameter> { new SqlParameter("@table_name", tableName) });
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

        /// <summary>
        /// 判断是否存在存储过程
        /// </summary>
        /// <param name="proceName">存储过程名</param>
        /// <returns></returns>
        public override bool ExistsProcedure(string proceName)
        {
            string sql = $"select * from dbo.sysobjects where id =object_id(N'[dbo].[{proceName}]') and OBJECTPROPERTY(id, N'IsProcedure')= 1";
            DataTable table = GetDataTableWithSql(sql);
            return table.Rows.Count > 0;
        }

        /// <summary>
        /// 复制表结构
        /// </summary>
        /// <param name="sourceTable">原表</param>
        /// <param name="targetTable">目标表</param>
        public override void CloneTableStructure(string sourceTable, string targetTable)
        {
            string proceName = "spCloneTableStructure";
            if (!ExistsProcedure(proceName))
            {
                //参考https://stackoverflow.com/a/38405661
                string sql = @"CREATE PROCEDURE [dbo].[spCloneTableStructure]

@SourceTable            nvarchar(255),
@DestinationTable       nvarchar(255),
@PartionField           nvarchar(255) = '',
@SourceSchema           nvarchar(255) = 'dbo',  
@DestinationSchema      nvarchar(255) = 'dbo',    
@RecreateIfExists       bit = 1

AS
BEGIN

DECLARE @msg  nvarchar(200), @PartionScript nvarchar(255), @sql NVARCHAR(MAX)

    IF EXISTS(Select s.name As SchemaName, t.name As TableName
                        From sys.tables t
                        Inner Join sys.schemas s On t.schema_id = s.schema_id
                        Inner Join sys.partitions p on p.object_id = t.object_id
                        Where p.index_id In (0, 1) and t.name = @SourceTable
                        Group By s.name, t.name
                        Having Count(*) > 1)

        SET @PartionScript = ' ON [PS_PartitionByCompanyId]([' + @PartionField + '])'
    else
        SET @PartionScript = ''

SET NOCOUNT ON;
BEGIN TRY   
    SET @msg ='  CloneTable  ' + @DestinationTable + ' - Step 1, Drop table if exists. Timestamp: '  + CONVERT(NVARCHAR(50),GETDATE(),108)
     RAISERROR( @msg,0,1) WITH NOWAIT
    --drop the table
    if EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @DestinationTable)
    BEGIN
        if @RecreateIfExists = 1
            BEGIN
                exec('DROP TABLE [' + @DestinationSchema + '].[' + @DestinationTable + ']')
            END
        ELSE
            RETURN
    END

    SET @msg ='  CloneTable  ' + @DestinationTable + ' - Step 2, Create table. Timestamp: '  + CONVERT(NVARCHAR(50),GETDATE(),108)
    RAISERROR( @msg,0,1) WITH NOWAIT
    --create the table
    exec('SELECT TOP (0) * INTO [' + @DestinationTable + '] FROM [' + @SourceTable + ']')       

    --create primary key
    SET @msg ='  CloneTable  ' + @DestinationTable + ' - Step 3, Create primary key. Timestamp: '  + CONVERT(NVARCHAR(50),GETDATE(),108)
    RAISERROR( @msg,0,1) WITH NOWAIT
    DECLARE @PKSchema nvarchar(255), @PKName nvarchar(255),@count   INT
    SELECT TOP 1 @PKSchema = CONSTRAINT_SCHEMA, @PKName = CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = @SourceSchema AND TABLE_NAME = @SourceTable AND CONSTRAINT_TYPE = 'PRIMARY KEY'
    IF NOT @PKSchema IS NULL AND NOT @PKName IS NULL
    BEGIN
        DECLARE @PKColumns nvarchar(MAX)
        SET @PKColumns = ''

        SELECT @PKColumns = @PKColumns + '[' + COLUMN_NAME + '],'
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
            where TABLE_NAME = @SourceTable and TABLE_SCHEMA = @SourceSchema AND CONSTRAINT_SCHEMA = @PKSchema AND CONSTRAINT_NAME= @PKName
            ORDER BY ORDINAL_POSITION

        SET @PKColumns = LEFT(@PKColumns, LEN(@PKColumns) - 1)

        exec('ALTER TABLE [' + @DestinationSchema + '].[' + @DestinationTable + '] ADD  CONSTRAINT [PK_' + @DestinationTable + '] PRIMARY KEY CLUSTERED (' + @PKColumns + ')' + @PartionScript);
    END

    --create other indexes
    SET @msg ='  CloneTable  ' + @DestinationTable + ' - Step 4, Create Indexes. Timestamp: '  + CONVERT(NVARCHAR(50),GETDATE(),108)
    RAISERROR( @msg,0,1) WITH NOWAIT
    DECLARE @IndexId int, @IndexName nvarchar(255), @IsUnique bit, @IsUniqueConstraint bit, @FilterDefinition nvarchar(max), @type int

    set @count=0
    DECLARE indexcursor CURSOR FOR
    SELECT index_id, name, is_unique, is_unique_constraint, filter_definition, type FROM sys.indexes WHERE is_primary_key = 0 and object_id = object_id('[' + @SourceSchema + '].[' + @SourceTable + ']')
    OPEN indexcursor;
    FETCH NEXT FROM indexcursor INTO @IndexId, @IndexName, @IsUnique, @IsUniqueConstraint, @FilterDefinition, @type
    WHILE @@FETCH_STATUS = 0
       BEGIN
            set @count =@count +1
            DECLARE @Unique nvarchar(255)
            SET @Unique = CASE WHEN @IsUnique = 1 THEN ' UNIQUE ' ELSE '' END

            DECLARE @KeyColumns nvarchar(max), @IncludedColumns nvarchar(max)
            SET @KeyColumns = ''
            SET @IncludedColumns = ''

            select @KeyColumns = @KeyColumns + '[' + c.name + '] ' + CASE WHEN is_descending_key = 1 THEN 'DESC' ELSE 'ASC' END + ',' from sys.index_columns ic
            inner join sys.columns c ON c.object_id = ic.object_id and c.column_id = ic.column_id
            where index_id = @IndexId and ic.object_id = object_id('[' + @SourceSchema + '].[' + @SourceTable + ']') and key_ordinal > 0
            order by index_column_id

            select @IncludedColumns = @IncludedColumns + '[' + c.name + '],' from sys.index_columns ic
            inner join sys.columns c ON c.object_id = ic.object_id and c.column_id = ic.column_id
            where index_id = @IndexId and ic.object_id = object_id('[' + @SourceSchema + '].[' + @SourceTable + ']') and key_ordinal = 0
            order by index_column_id

            IF LEN(@KeyColumns) > 0
                SET @KeyColumns = LEFT(@KeyColumns, LEN(@KeyColumns) - 1)

            IF LEN(@IncludedColumns) > 0
            BEGIN
                SET @IncludedColumns = ' INCLUDE (' + LEFT(@IncludedColumns, LEN(@IncludedColumns) - 1) + ')'
            END

            IF @FilterDefinition IS NULL
                SET @FilterDefinition = ''
            ELSE
                SET @FilterDefinition = 'WHERE ' + @FilterDefinition + ' '

            SET @msg ='  CloneTable  ' + @DestinationTable + ' - Step 4.' + CONVERT(NVARCHAR(5),@count) + ', Create Index ' + @IndexName + '. Timestamp: '  + CONVERT(NVARCHAR(50),GETDATE(),108)
            RAISERROR( @msg,0,1) WITH NOWAIT

            if @type = 2
                SET @sql = 'CREATE ' + @Unique + ' NONCLUSTERED INDEX [' + @IndexName + '] ON [' + @DestinationSchema + '].[' + @DestinationTable + '] (' + @KeyColumns + ')' + @IncludedColumns + @FilterDefinition  + @PartionScript
            ELSE
                BEGIN
                    SET @sql = 'CREATE ' + @Unique + ' CLUSTERED INDEX [' + @IndexName + '] ON [' + @DestinationSchema + '].[' + @DestinationTable + '] (' + @KeyColumns + ')' + @IncludedColumns + @FilterDefinition + @PartionScript
                END
            EXEC (@sql)
            FETCH NEXT FROM indexcursor INTO @IndexId, @IndexName, @IsUnique, @IsUniqueConstraint, @FilterDefinition, @type
       END
    CLOSE indexcursor
    DEALLOCATE indexcursor

    --create constraints
    SET @msg ='  CloneTable  ' + @DestinationTable + ' - Step 5, Create constraints. Timestamp: '  + CONVERT(NVARCHAR(50),GETDATE(),108)
    RAISERROR( @msg,0,1) WITH NOWAIT
    DECLARE @ConstraintName nvarchar(max), @CheckClause nvarchar(max), @ColumnName NVARCHAR(255)
    DECLARE const_cursor CURSOR FOR
        SELECT
            REPLACE(dc.name, @SourceTable, @DestinationTable),[definition], c.name
        FROM sys.default_constraints dc
            INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
        WHERE OBJECT_NAME(parent_object_id) =@SourceTable               
    OPEN const_cursor
    FETCH NEXT FROM const_cursor INTO @ConstraintName, @CheckClause, @ColumnName
    WHILE @@FETCH_STATUS = 0
       BEGIN
            exec('ALTER TABLE [' + @DestinationTable + '] ADD CONSTRAINT [' + @ConstraintName + '] DEFAULT ' + @CheckClause + ' FOR ' + @ColumnName)
            FETCH NEXT FROM const_cursor INTO @ConstraintName, @CheckClause, @ColumnName
       END;
    CLOSE const_cursor
    DEALLOCATE const_cursor                 


END TRY
    BEGIN CATCH
        IF (SELECT CURSOR_STATUS('global','indexcursor')) >= -1
        BEGIN
         DEALLOCATE indexcursor
        END

        IF (SELECT CURSOR_STATUS('global','const_cursor')) >= -1
        BEGIN
         DEALLOCATE const_cursor
        END


        PRINT 'Error Message: ' + ERROR_MESSAGE(); 
    END CATCH

END";
                ExecuteSql(sql);
            }

            string newSql = $"EXEC {proceName} '{sourceTable}','{targetTable}'";
            ExecuteSql(newSql);
        }

        #endregion
    }
}
