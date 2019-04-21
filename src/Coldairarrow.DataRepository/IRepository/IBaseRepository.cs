using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Coldairarrow.DataRepository
{
    public interface IBaseRepository
    {
        #region 增加数据

        void Insert<T>(T entity) where T : class, new();
        void Insert<T>(List<T> entities) where T : class, new();

        #endregion

        #region 删除数据

        void Delete<T>(string key) where T : class, new();
        void Delete<T>(List<string> keys) where T : class, new();
        void Delete<T>(T entity) where T : class, new();
        void Delete<T>(List<T> entities) where T : class, new();
        void Delete<T>(Expression<Func<T, bool>> condition) where T : class, new();
        void Delete_Sql<T>(Expression<Func<T, bool>> condition) where T : class, new();

        #endregion

        #region 更新数据

        void Update<T>(T entity) where T : class, new();
        void Update<T>(List<T> entities) where T : class, new();
        void UpdateAny<T>(T entity, List<string> properties) where T : class, new();
        void UpdateAny<T>(List<T> entities, List<string> properties) where T : class, new();
        void UpdateWhere<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new();

        #endregion

        #region 查询数据

        T GetEntity<T>(params object[] keyValue) where T : class, new();
        List<T> GetList<T>() where T : class, new();

        #endregion
    }
}
