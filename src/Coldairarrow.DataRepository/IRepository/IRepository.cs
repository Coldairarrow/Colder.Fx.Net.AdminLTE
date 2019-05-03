using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public interface IRepository : IBaseRepository, ITransaction
    {
        #region 数据库连接相关方法

        DbContext GetDbContext();
        Action<string> HandleSqlLog { get; set; }

        #endregion

        #region 增加数据

        void Insert(List<object> entities);
        void BulkInsert<T>(List<T> entities) where T : class, new();

        #endregion

        #region 删除数据

        void DeleteAll<T>() where T : class, new();
        void DeleteAll(Type type);
        void Delete(Type type, string key);
        void Delete(Type type, List<string> keys);
        void Delete(List<object> entities);
        void Delete<T>(string key) where T : class, new();
        void Delete<T>(List<string> keys) where T : class, new();
        void Delete_Sql<T>(Expression<Func<T, bool>> condition) where T : class, new();

        #endregion

        #region 更新数据

        void Update(List<object> entities);

        #endregion

        #region 查询数据

        T GetEntity<T>(params object[] keyValue) where T : class, new();
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
