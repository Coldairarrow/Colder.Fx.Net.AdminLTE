using System.Linq;
using System.Linq.Dynamic;

namespace Coldairarrow.Util
{
    /// <summary>
    /// IQueryable"T"的拓展操作
    /// 作者：Coldairarrow
    /// </summary>
    public static partial class Extention
    {
        /// <summary>
        /// 获取分页后的数据
        /// </summary>
        /// <typeparam name="T">实体参数</typeparam>
        /// <param name="source">IQueryable数据源</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageRows">每页行数</param>
        /// <param name="orderColumn">排序列</param>
        /// <param name="orderType">排序类型</param>
        /// <param name="count">总记录数</param>
        /// <param name="pages">总页数</param>
        /// <returns></returns>
        public static IQueryable<T> GetPagination<T>(this IQueryable<T> source, int pageIndex, int pageRows, string orderColumn, string orderType, ref int count, ref int pages)
        {
            Pagination pagination = new Pagination
            {
                page = pageIndex,
                rows = pageRows,
                sord = orderType,
                sidx = orderColumn
            };

            return source.GetPagination(pagination);
        }

        /// <summary>
        /// 获取分页后的数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源IQueryable</param>
        /// <param name="pagination">分页参数</param>
        /// <returns></returns>
        public static IQueryable<T> GetPagination<T>(this IQueryable<T> source, Pagination pagination)
        {
            pagination.records = source.Count();
            source = source.OrderBy($"{pagination.sidx} {pagination.sord}");
            return source.Skip((pagination.page - 1) * pagination.rows).Take(pagination.rows);
        }

        /// <summary>
        /// 获取分页后的数据
        /// </summary>
        /// <param name="source">数据源IQueryable</param>
        /// <param name="pagination">分页参数</param>
        /// <returns></returns>
        public static IQueryable GetPagination(this IQueryable source, Pagination pagination)
        {
            pagination.records = source.Count();
            source = source.OrderBy($"{pagination.sidx} {pagination.sord}");
            return source.Skip((pagination.page - 1) * pagination.rows).Take(pagination.rows);
        }

        /// <summary>
        /// 动态排序法
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">IQueryable数据源</param>
        /// <param name="sortColumn">排序的列</param>
        /// <param name="sortType">排序的方法</param>
        /// <returns></returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string sortColumn, string sortType)
        {
            return source.OrderBy(string.Format("{0} {1}", sortColumn, sortType));
        }

        /// <summary>
        /// 拓展IQueryable"T"方法操作
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> AsExpandable<T>(this IQueryable<T> source)
        {
            return LinqKit.Extensions.AsExpandable(source);
        }
    }
}
