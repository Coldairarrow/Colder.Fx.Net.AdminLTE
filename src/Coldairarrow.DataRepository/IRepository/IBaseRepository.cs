using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 基数据仓储
    /// </summary>
    public interface IBaseRepository
    {
        #region 增加数据

        /// <summary>
        /// 添加单条记录
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <param name="entity">实体对象</param>
        void Insert<T>(T entity) where T : class, new();

        /// <summary>
        /// 添加多条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void Insert<T>(List<T> entities) where T : class, new();

        #endregion

        #region 删除数据

        void Delete<T>(T entity) where T : class, new();
        void Delete<T>(List<T> entities) where T : class, new();
        void Delete<T>(Expression<Func<T, bool>> condition) where T : class, new();

        #endregion

        #region 更新数据

        void Update<T>(T entity) where T : class, new();
        void Update<T>(List<T> entities) where T : class, new();
        void UpdateAny<T>(T entity, List<string> properties) where T : class, new();
        void UpdateAny<T>(List<T> entities, List<string> properties) where T : class, new();
        void UpdateWhere<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new();

        #endregion

        #region 查询数据

        List<T> GetList<T>() where T : class, new();

        #endregion
    }
}
