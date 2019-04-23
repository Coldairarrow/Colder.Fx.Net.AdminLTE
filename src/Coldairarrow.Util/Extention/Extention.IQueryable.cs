using System;
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

        /// <summary>
        /// 获取Skip数量
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static int GetSkipCount(this IQueryable source)
        {
            var visitor = new SkipVisitor();
            visitor.Visit(source.Expression);

            return visitor.SkipCount;
        }

        /// <summary>
        /// 获取Take数量
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static int GetTakeCount(this IQueryable source)
        {
            var visitor = new TakeVisitor();
            visitor.Visit(source.Expression);

            return visitor.TakeCount;
        }

        /// <summary>
        /// 获取排序参数
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static (string sortColumn, string sortType) GetOrderBy(this IQueryable source)
        {
            var visitor = new OrderByVisitor();
            visitor.Visit(source.Expression);

            return visitor.OrderParam;
        }

        public static IQueryable ChangeSource(this IQueryable source, IQueryable targetSource)
        {
            var methods = GetMethods(source.Expression);
            var targetObjQuery = targetSource.GetObjQuery();
            Expression resExpression = (targetObjQuery as IQueryable).Expression;
            while (true)
            {
                if (methods.Count == 0)
                    break;
                var theMethod = methods.Pop();
                
            }

            ChangeSourceVisitor visitor = new ChangeSourceVisitor(source.GetObjQuery(), targetSource.GetObjQuery(), targetSource.ElementType);
            
            return targetSource.Provider.CreateQuery(visitor.Visit(source.Expression));

            Stack<MethodCallExpression> GetMethods(Expression expression)
            {
                Stack<MethodCallExpression> resList = new Stack<MethodCallExpression>();

                Expression next = expression;
                while (true)
                {
                    if (next is MethodCallExpression methodCall)
                    {
                        resList.Push(methodCall);
                        next = methodCall.Arguments[0];
                    }
                    else
                    {
                        break;
                    }
                }

                return resList;
            }
        }

        public static ObjectQuery GetObjQuery(this IQueryable source)
        {
            GetObjQueryVisitor visitor = new GetObjQueryVisitor();
            visitor.Visit(source.Expression);

            return visitor.ObjQuery;
        }

        class RemoveAllMethod : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                return node.Arguments[0];
            }
        }

        class ChangeSourceVisitor : ExpressionVisitor
        {
            public ChangeSourceVisitor(ObjectQuery oldObjQuery,ObjectQuery targetObjQuery, Type targetType)
            {
                _oldObjQuery = oldObjQuery;
                _targetObjQuery = targetObjQuery;
                _targetType = targetType;
                _targetObjQueryConstant = Expression.Constant(targetObjQuery);
            }
            private ObjectQuery _oldObjQuery { get; }
            private ObjectQuery _targetObjQuery { get; }
            private Type _targetType { get; }
            private ConstantExpression _targetObjQueryConstant { get; }
            private MethodInfo GetMethod(string methodName)
            {
                return _targetObjQuery.GetType().GetTypeInfo().DeclaredMethods.Where(x => x.Name == methodName).Single();
            }
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value == _oldObjQuery)
                    return _targetObjQueryConstant;

                return base.VisitConstant(node);
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "MergeAs")
                {
                    var arg = node.Arguments;
                    var method = GetMethod(node.Method.Name);

                    return Expression.Call(_targetObjQueryConstant, method, node.Arguments.ToArray());
                }
                if (node.Method.Name == "Where")
                {
                    var newParamter = Expression.Parameter(_targetType, "$x");
                    var whereVisitor = new RepleaseWhereLambdaVisitor(newParamter);
                    var newWhereLambdaBody = whereVisitor.Visit(((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body);
                    var lambda = Expression.Lambda(newWhereLambdaBody, newParamter);
                    return Expression.Call(
                        typeof(Queryable),
                        node.Method.Name,
                        new Type[] { _targetType },
                        new Expression[] { _targetObjQueryConstant, Expression.Quote(lambda) });
                }
                if (node.Method.Name == "OrderBy" || node.Method.Name == "OrderByDescending")
                {
                    var oldLambda = (node.Arguments[1] as UnaryExpression).Operand as LambdaExpression;
                    var newParamter = Expression.Parameter(_targetType, "$x");
                    var whereVisitor = new RepleaseOrderByLambdaVisitor(newParamter);
                    var newWhereLambdaBody = whereVisitor.Visit(oldLambda.Body);
                    var lambda = Expression.Lambda(newWhereLambdaBody, newParamter);
                    return Expression.Call(
                        typeof(Queryable),
                        node.Method.Name,
                        new Type[] { _targetType, oldLambda.ReturnType },
                        new Expression[] { _targetObjQueryConstant, Expression.Quote(lambda) });
                }

                return base.VisitMethodCall(node);
            }
        }

        class RepleaseWhereLambdaVisitor : ExpressionVisitor
        {
            public RepleaseWhereLambdaVisitor(ParameterExpression newParamter)
            {
                _newParamter = newParamter;
            }

            private ParameterExpression _newParamter { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _newParamter;
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return Expression.MakeMemberAccess(_newParamter, _newParamter.Type.GetMember(node.Member.Name).Single());
            }
        }

        class RepleaseOrderByLambdaVisitor : ExpressionVisitor
        {
            public RepleaseOrderByLambdaVisitor(ParameterExpression newParamter)
            {
                _newParamter = newParamter;
            }

            private ParameterExpression _newParamter { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _newParamter;
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return Expression.MakeMemberAccess(_newParamter, _newParamter.Type.GetMember(node.Member.Name).Single());
            }
        }

        class GetObjQueryVisitor : ExpressionVisitor
        {
            public ObjectQuery ObjQuery { get; set; }
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is ObjectQuery)
                    ObjQuery = node.Value as ObjectQuery;

                return base.VisitConstant(node);
            }
        }

        class OrderByVisitor : ExpressionVisitor
        {
            public (string sortColumn, string sortType) OrderParam { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "OrderBy" || node.Method.Name == "OrderByDescending")
                {
                    string sortColumn = (((node.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberExpression).Member.Name;
                    string sortType = node.Method.Name == "OrderBy" ? "asc" : "desc";
                    OrderParam = (sortColumn, sortType);
                }
                return base.VisitMethodCall(node);
            }
        }

        class SkipVisitor : ExpressionVisitor
        {
            public int SkipCount { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Skip")
                {
                    SkipCount = (int)(node.Arguments[1] as ConstantExpression).Value;
                }
                return base.VisitMethodCall(node);
            }
        }

        class TakeVisitor : ExpressionVisitor
        {
            public int TakeCount { get; set; }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Take")
                {
                    TakeCount = (int)(node.Arguments[1] as ConstantExpression).Value;
                }
                return base.VisitMethodCall(node);
            }
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

        /// <summary>
        /// 删除OrderBy表达式
        /// </summary>
        public class RemoveOrderByVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType != typeof(Enumerable) && node.Method.DeclaringType != typeof(Queryable))
                    return base.VisitMethodCall(node);

                if (node.Method.Name != "OrderBy" && node.Method.Name != "OrderByDescending" && node.Method.Name != "ThenBy" && node.Method.Name != "ThenByDescending")
                    return base.VisitMethodCall(node);

                //eliminate the method call from the expression tree by returning the object of the call.
                return base.Visit(node.Arguments[0]);
            }
        }

        /// <summary>
        /// 删除Skip表达式
        /// </summary>
        public class RemoveSkipVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType != typeof(Enumerable) && node.Method.DeclaringType != typeof(Queryable))
                    return base.VisitMethodCall(node);

                if (node.Method.Name != "Skip")
                    return base.VisitMethodCall(node);

                //eliminate the method call from the expression tree by returning the object of the call.
                return base.Visit(node.Arguments[0]);
            }
        }

        /// <summary>
        /// 删除Take表达式
        /// </summary>
        public class RemoveTakeVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType != typeof(Enumerable) && node.Method.DeclaringType != typeof(Queryable))
                    return base.VisitMethodCall(node);

                if (node.Method.Name != "Take")
                    return base.VisitMethodCall(node);

                //eliminate the method call from the expression tree by returning the object of the call.
                return base.Visit(node.Arguments[0]);
            }
        }
    }
}
