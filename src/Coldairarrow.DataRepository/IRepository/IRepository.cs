using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace Coldairarrow.DataRepository
{
    public interface IRepository : IBaseRepository
    {
        #region 数据库连接相关方法

        DbContext GetDbContext();
        Action<string> HandleSqlLog { get; set; }

        #endregion

        #region 事物提交

        void BeginTransaction();
        bool EndTransaction();

        #endregion

        #region 增加数据

        void Insert(object entity);
        void Insert(List<object> entities);
        void BulkInsert<T>(List<T> entities) where T : class, new();

        #endregion

        #region 删除数据

        void DeleteAll<T>() where T : class, new();
        void DeleteAll(Type type);
        void Delete(Type type, string key);
        void Delete(Type type, List<string> keys);
        void Delete(object entity);
        void Delete(List<object> entities);

        #endregion

        #region 更新数据

        void Update(object entity);
        void Update(List<object> entities);

        #endregion

        #region 查询数据

        object GetEntity(Type type, params object[] keyValue);
        List<object> GetList(Type type);
        IQueryable<T> GetIQueryable<T>() where T : class, new();
        IQueryable GetIQueryable(Type type);
        DataTable GetDataTableWithSql(string sql);
        DataTable GetDataTableWithSql(string sql, List<DbParameter> parameters);
        List<T> GetListBySql<T>(string sqlStr) where T : class, new();
        List<T> GetListBySql<T>(string sqlStr, List<DbParameter> parameters) where T : class, new();

        #endregion

        #region 执行Sql语句

        void ExecuteSql(string sql);
        void ExecuteSql(string sql, List<DbParameter> parameters);

        #endregion
    }
}
