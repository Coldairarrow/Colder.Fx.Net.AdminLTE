using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public interface IRepository : IBaseRepository, ITransaction, IDisposable
    {
        #region 数据库连接相关方法

        /// <summary>
        /// 获取DbContext
        /// </summary>
        /// <returns></returns>
        DbContext GetDbContext();

        /// <summary>
        /// SQL日志处理方法
        /// </summary>
        /// <value>
        /// The handle SQL log.
        /// </value>
        Action<string> HandleSqlLog { get; set; }

        /// <summary>
        /// 提交到数据库
        /// </summary>
        void CommitDb();

        #endregion

        #region 增加数据

        /// <summary>
        /// 添加多条记录
        /// </summary>
        /// <param name="entities">对象集合</param>
        void Insert(List<object> entities);

        /// <summary>
        /// 使用Bulk批量导入,速度快
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="entities">实体集合</param>
        void BulkInsert<T>(List<T> entities) where T : class, new();

        #endregion

        #region 删除数据

        /// <summary>
        /// 删除所有记录
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        void DeleteAll<T>() where T : class, new();

        /// <summary>
        /// Deletes all.
        /// </summary>
        /// <param name="type">The type.</param>
        void DeleteAll(Type type);

        /// <summary>
        /// Deletes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        void Delete(Type type, string key);

        /// <summary>
        /// Deletes the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="keys">The keys.</param>
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
