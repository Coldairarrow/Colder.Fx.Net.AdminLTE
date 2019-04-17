using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

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

        /// <summary>
        /// 删除OrderBy表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> RemoveOrderBy<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)((IQueryable)source).RemoveOrderBy();
        }

        /// <summary>
        /// 删除OrderBy表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable RemoveOrderBy(this IQueryable source)
        {
            return source.Provider.CreateQuery(new RemoveOrderByVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> RemoveSkip<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)((IQueryable)source).RemoveSkip();
        }

        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable RemoveSkip(this IQueryable source)
        {
            return source.Provider.CreateQuery(new RemoveSkipVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable<T> RemoveTake<T>(this IQueryable<T> source)
        {
            return (IQueryable<T>)((IQueryable)source).RemoveTake();
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static IQueryable RemoveTake(this IQueryable source)
        {
            return source.Provider.CreateQuery(new RemoveTakeVisitor().Visit(source.Expression));
        }

        /// <summary>
        /// 切换DbContext
        /// </summary>
        /// <typeparam name="T">数据源类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="target">目标DbContext</param>
        /// <returns></returns>
        public static IQueryable<T> ChangeDbContext<T>(this IQueryable<T> source, DbContext target)
        {
            var binder = new ChangeDbContextVisitor(target);
            var expression = binder.Visit(source.Expression);
            var provider = binder.TargetProvider;
            return provider != null ? provider.CreateQuery<T>(expression) : source;
        }

        /// <summary>
        /// 转为获取对应的SQL语句
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static (string sql, List<ObjectParameter> parameters) ToSQL(this IQueryable source)
        {
            var dbQuery = (DbQuery)source;
            var iqProp = dbQuery.GetType().GetProperty("InternalQuery", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var iq = iqProp.GetValue(dbQuery, null);
            var oqProp = iq.GetType().GetProperty("ObjectQuery", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var objectQuery = (ObjectQuery)oqProp.GetValue(iq, null);

            return (objectQuery.ToTraceString(), objectQuery.Parameters.ToList());
        }
        
        class ChangeDbContextVisitor : ExpressionVisitor
        {
            public ChangeDbContextVisitor(DbContext target)
            {
                targetObjectContext = ((IObjectContextAdapter)target).ObjectContext;
            }
            ObjectContext targetObjectContext;
            public IQueryProvider TargetProvider { get; private set; }
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is ObjectQuery objectQuery && objectQuery.Context != targetObjectContext)
                    return Expression.Constant(CreateObjectQuery((dynamic)objectQuery));
                return base.VisitConstant(node);
            }
            ObjectQuery<T> CreateObjectQuery<T>(ObjectQuery<T> source)
            {
                var parameters = source.Parameters
                    .Select(p => new ObjectParameter(p.Name, p.ParameterType) { Value = p.Value })
                    .ToArray();
                var query = targetObjectContext.CreateQuery<T>(source.CommandText, parameters);
                query.MergeOption = source.MergeOption;
                query.Streaming = source.Streaming;
                query.EnablePlanCaching = source.EnablePlanCaching;
                if (TargetProvider == null)
                    TargetProvider = ((IQueryable)query).Provider;
                return query;
            }
        }
    }
}
