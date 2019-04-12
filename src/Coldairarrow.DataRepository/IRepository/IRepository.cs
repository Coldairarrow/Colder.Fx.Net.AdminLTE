using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public interface IRepository
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
        void Insert<T>(T entity) where T : class, new();
        void Insert(List<object> entities);
        void Insert<T>(List<T> entities) where T : class, new();
        void BulkInsert<T>(List<T> entities) where T : class, new();

        #endregion

        #region 删除数据

        void DeleteAll<T>() where T : class, new();
        void Delete<T>(string key) where T : class, new();
        void Delete<T>(List<string> keys) where T : class, new();
        void Delete<T>(T entity) where T : class, new();
        void Delete<T>(List<T> entities) where T : class, new();
        void Delete<T>(Expression<Func<T, bool>> condition) where T : class, new();
        void Delete_Sql<T>(Expression<Func<T, bool>> condition) where T : class, new();
        void DeleteAll(Type type);
        void Delete(Type type, string key);
        void Delete(Type type, List<string> keys);
        void Delete(object entity);
        void Delete(List<object> entities);

        #endregion

        #region 更新数据

        void Update(object entity);
        void Update<T>(T entity) where T : class, new();
        void Update(List<object> entities);
        void Update<T>(List<T> entities) where T : class, new();
        void UpdateAny(object entity, List<string> properties);
        void UpdateAny<T>(T entity, List<string> properties) where T : class, new();
        void UpdateAny(List<object> entities, List<string> properties);
        void UpdateAny<T>(List<T> entities, List<string> properties) where T : class, new();
        void UpdateWhere<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new();

        #endregion

        #region 查询数据

        object GetEntity(Type type, params object[] keyValue);
        T GetEntity<T>(params object[] keyValue) where T : class, new();
        List<object> GetList(Type type);
        List<T> GetList<T>() where T : class, new();
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
