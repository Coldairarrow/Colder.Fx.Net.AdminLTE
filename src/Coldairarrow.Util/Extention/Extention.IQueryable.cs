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
        #region 外部接口

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
        public static int? GetSkipCount(this IQueryable source)
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
        public static int? GetTakeCount(this IQueryable source)
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

        /// <summary>
        /// 切换数据源,保留原数据源中的Expression
        /// </summary>
        /// <param name="source">原数据源</param>
        /// <param name="targetSource">目标数据源</param>
        /// <returns></returns>
        public static IQueryable ChangeSource(this IQueryable source, IQueryable targetSource)
        {
            if (!(source is DbQuery && targetSource is DbQuery))
                throw new Exception("仅支持EF的IQueryable!");

            Dictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
            var oldQuery = source.GetObjQuery() as IQueryable;
            var newQuery = targetSource.GetObjQuery() as IQueryable;
            typeMap[oldQuery.ElementType] = newQuery.ElementType;
            var methods = GetMethods(source.Expression);
            Expression newExpression = newQuery.Expression;
            Expression oldExpression = oldQuery.Expression;

            while (true)
            {
                if (methods.Count == 0)
                    break;
                var theMethod = methods.Pop();
                string methodName = theMethod.Method.Name;
                if (theMethod.Method.Name == "MergeAs")
                    continue;

                var args = theMethod.Arguments.ToList();
                args[0] = newExpression;
                for (int i = 1; i < args.Count; i++)
                {
                    args[i] = new ArgumentVisitor(ChangeSource_BuildParamters(args[i], typeMap), typeMap).Visit(args[i]);
                }
                var genericArguments = theMethod.Method.GetGenericArguments().ToList();
                for (int i = 0; i < genericArguments.Count; i++)
                {
                    if (typeMap.ContainsKey(genericArguments[i]))
                        genericArguments[i] = typeMap[genericArguments[i]];
                }

                newExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    genericArguments.ToArray(),
                    args.ToArray());
                newQuery = newQuery.Provider.CreateQuery(newExpression);
                oldExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    theMethod.Method.GetGenericArguments(),
                    theMethod.Arguments.ToArray());

                oldQuery = oldQuery.Provider.CreateQuery(oldExpression);

                typeMap[oldQuery.ElementType] = newQuery.ElementType;
            }

            return targetSource.Provider.CreateQuery(newExpression);

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

        /// <summary>
        /// 获取ObjectQuery
        /// </summary>
        /// <param name="source">数据源</param>
        /// <returns></returns>
        public static ObjectQuery GetObjQuery(this IQueryable source)
        {
            GetObjQueryVisitor visitor = new GetObjQueryVisitor();
            visitor.Visit(source.Expression);

            return visitor.ObjQuery;
        }

        public static TResult Max<TSource, TResult>(this IQueryable source, Expression<Func<TSource, TResult>> selector)
        {
            return (TResult)DynamicMax(source, selector);
        }
        public static TResult Min<TSource, TResult>(this IQueryable source, Expression<Func<TSource, TResult>> selector)
        {
            return (TResult)DynamicMin(source, selector);
        }
        public static double Average<TSource>(this IQueryable source, Expression<Func<TSource, int>> selector)
        {
            return (double)DynamicAverage(source, selector);
        }
        public static double? Average<TSource>(this IQueryable source, Expression<Func<TSource, int?>> selector)
        {
            return (double?)DynamicAverage(source, selector);
        }
        public static float Average<TSource>(this IQueryable source, Expression<Func<TSource, float>> selector)
        {
            return (float)DynamicAverage(source, selector);
        }
        public static float? Average<TSource>(this IQueryable source, Expression<Func<TSource, float?>> selector)
        {
            return (float?)DynamicAverage(source, selector);
        }
        public static double Average<TSource>(this IQueryable source, Expression<Func<TSource, long>> selector)
        {
            return (double)DynamicAverage(source, selector);
        }
        public static double? Average<TSource>(this IQueryable source, Expression<Func<TSource, long?>> selector)
        {
            return (double?)DynamicAverage(source, selector);
        }
        public static double Average<TSource>(this IQueryable source, Expression<Func<TSource, double>> selector)
        {
            return (double)DynamicAverage(source, selector);
        }
        public static double? Average<TSource>(this IQueryable source, Expression<Func<TSource, double?>> selector)
        {
            return (double?)DynamicAverage(source, selector);
        }
        public static decimal Average<TSource>(this IQueryable source, Expression<Func<TSource, decimal>> selector)
        {
            return (decimal)DynamicAverage(source, selector);
        }
        public static decimal? Average<TSource>(this IQueryable source, Expression<Func<TSource, decimal?>> selector)
        {
            return (decimal?)DynamicAverage(source, selector);
        }
        public static decimal Sum<TSource>(this IQueryable source, Expression<Func<TSource, decimal>> selector)
        {
            return (decimal)DynamicSum(source, selector);
        }
        public static decimal? Sum<TSource>(this IQueryable source, Expression<Func<TSource, decimal?>> selector)
        {
            return (decimal?)DynamicSum(source, selector);
        }
        public static double Sum<TSource>(this IQueryable source, Expression<Func<TSource, double>> selector)
        {
            return (double)DynamicSum(source, selector);
        }
        public static double? Sum<TSource>(this IQueryable source, Expression<Func<TSource, double?>> selector)
        {
            return (double?)DynamicSum(source, selector);
        }
        public static float Sum<TSource>(this IQueryable source, Expression<Func<TSource, float>> selector)
        {
            return (float)DynamicSum(source, selector);
        }
        public static float? Sum<TSource>(this IQueryable source, Expression<Func<TSource, float?>> selector)
        {
            return (float?)DynamicSum(source, selector);
        }
        public static int Sum<TSource>(this IQueryable source, Expression<Func<TSource, int>> selector)
        {
            return (int)DynamicSum(source, selector);
        }
        public static int? Sum<TSource>(this IQueryable source, Expression<Func<TSource, int?>> selector)
        {
            return (int?)DynamicSum(source, selector);
        }
        public static long Sum<TSource>(this IQueryable source, Expression<Func<TSource, long>> selector)
        {
            return (long)DynamicSum(source, selector);
        }
        public static long? Sum<TSource>(this IQueryable source, Expression<Func<TSource, long?>> selector)
        {
            return (long?)DynamicSum(source, selector);
        }
        public static dynamic DynamicSum(this IQueryable source, dynamic selector)
        {
            ParameterExpression newParamter = Expression.Parameter(source.ElementType, "x");
            var newBody = new StatisVisitor(newParamter).Visit(selector.Body);
            dynamic dynSelector = Expression.Lambda(newBody, newParamter);
            dynamic dynSource = source;

            return Queryable.Sum(dynSource, dynSelector);
        }

        #endregion

        #region 自定义类

        class StatisVisitor : ExpressionVisitor
        {
            public StatisVisitor(ParameterExpression newParamter)
            {
                _newParamter = newParamter;
            }
            ParameterExpression _newParamter { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _newParamter;
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return Expression.MakeMemberAccess(_newParamter, _newParamter.Type.GetMember(node.Member.Name).Single());
            }
        }

        class ArgumentVisitor : ExpressionVisitor
        {
            public ArgumentVisitor(
                Dictionary<string, ParameterExpression> paramters,
                Dictionary<Type, Type> typeMap)
            {
                _paramters = paramters;
                _typeMap = typeMap;
            }
            Dictionary<string, ParameterExpression> _paramters { get; }
            Dictionary<Type, Type> _typeMap { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ChangeSource_VisitParameter(node, _paramters);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return ChangeSource_VisitMember(node, _paramters);
            }
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                var newParamters = ChangeSource_BuildParamters(node, _typeMap);
                return ChangeSource_VisitLambda(node, newParamters, _typeMap);
            }
        }
        class LambdaVisitor : ExpressionVisitor
        {
            public LambdaVisitor(
                Dictionary<string, ParameterExpression> paramters,
                Dictionary<Type, Type> typeMap)
            {
                _paramters = paramters;
                _typeMap = typeMap;
            }
            Dictionary<string, ParameterExpression> _paramters { get; }
            Dictionary<Type, Type> _typeMap { get; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ChangeSource_VisitParameter(node, _paramters);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                return ChangeSource_VisitMember(node, _paramters);
            }
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                string paramterName = node.Arguments[0] is ParameterExpression oldParamter ? oldParamter.Name : "";
                if (!_paramters.ContainsKey(paramterName))
                    return base.VisitMethodCall(node);

                return ChangeSource_VisitMethodCall(node, _typeMap, _paramters);
            }
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return ChangeSource_VisitLambda(node, _paramters, _typeMap);
            }
        }

        class GetParamtersVisitor : ExpressionVisitor
        {
            public List<ParameterExpression> Paramters { get; set; } = new List<ParameterExpression>();
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (!Paramters.Contains(node))
                    Paramters.Add(node);
                return base.VisitParameter(node);
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
            public int? SkipCount { get; set; }
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
            public int? TakeCount { get; set; }
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
                    return new ConstantVisitor(node.Arguments[1] as ConstantExpression, Expression.Constant(0)).Visit(node);

                //return Expression.Call(
                //        typeof(Queryable),
                //        node.Method.Name,
                //        node.Method.GetGenericArguments(),
                //        new Expression[] { node.Arguments[0], Expression.Constant(0) });

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
                {
                    return new ConstantVisitor(node.Arguments[1] as ConstantExpression, Expression.Constant(0)).Visit(node);
                    return Expression.Call(
    typeof(Queryable),
    node.Method.Name,
    node.Method.GetGenericArguments(),
    new Expression[] { node.Arguments[0], Expression.Constant(0) });

                }

                //eliminate the method call from the expression tree by returning the object of the call.
                return base.Visit(node.Arguments[0]);
            }
        }

        public class ConstantVisitor: ExpressionVisitor
        {
            public ConstantVisitor(ConstantExpression oldConstant, ConstantExpression newConstant)
            {
                _oldConstant = oldConstant;
                _newConstant = newConstant;
            }
            private ConstantExpression _oldConstant { get; }
            private ConstantExpression _newConstant { get; }
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node == _oldConstant)
                    return _newConstant;

                return base.VisitConstant(node);
            }
        }

        #endregion

        #region 私有成员

        private static Dictionary<string, ParameterExpression> ChangeSource_BuildParamters(Expression expression, Dictionary<Type, Type> map)
        {
            Dictionary<string, ParameterExpression> res = new Dictionary<string, ParameterExpression>();
            GetParamtersVisitor visitor = new GetParamtersVisitor();
            visitor.Visit(expression);
            var paramters = visitor.Paramters;
            paramters.ForEach(aParamter =>
            {
                if (!res.ContainsKey(aParamter.Name))
                {
                    if (map.ContainsKey(aParamter.Type))
                    {
                        res[aParamter.Name] = Expression.Parameter(map[aParamter.Type], aParamter.Name);
                    }
                    else
                    {
                        res[aParamter.Name] = aParamter;
                    }
                }
            });

            return res;
        }
        private static Expression ChangeSource_VisitParameter(ParameterExpression node, Dictionary<string, ParameterExpression> paramters)
        {
            var newNode = node;
            if (paramters.ContainsKey(node.Name))
                newNode = paramters[node.Name];

            return newNode;
        }
        private static Expression ChangeSource_VisitMember(MemberExpression node, Dictionary<string, ParameterExpression> paramters)
        {
            MemberExpression newNode = node;
            if (node.Expression is ParameterExpression oldParamter)
            {
                if (paramters.ContainsKey(oldParamter.Name))
                {
                    var newParamter = paramters[oldParamter.Name];
                    newNode = Expression.MakeMemberAccess(newParamter, newParamter.Type.GetMember(node.Member.Name).Single());
                }
            }

            return newNode;
        }
        private static Expression ChangeSource_VisitLambda<T>(Expression<T> node, Dictionary<string, ParameterExpression> paramters, Dictionary<Type, Type> typeMap)
        {
            var lambdaVisitor = new LambdaVisitor(paramters, typeMap);
            var newLambdaBody = lambdaVisitor.Visit(node.Body);
            var theParamters = node.Parameters.Select(x => paramters[x.Name]).ToArray();
            var lambda = Expression.Lambda(newLambdaBody, theParamters);

            return lambda;
        }
        private static Expression ChangeSource_VisitMethodCall(MethodCallExpression node, Dictionary<Type, Type> typeMap, Dictionary<string, ParameterExpression> paramters = null)
        {
            var theMethod = node;
            var args = theMethod.Arguments.ToList();
            for (int i = 0; i < args.Count; i++)
            {
                args[i] = new ArgumentVisitor(paramters ?? ChangeSource_BuildParamters(args[i], typeMap), typeMap).Visit(args[i]);
            }
            var genericArguments = theMethod.Method.GetGenericArguments().ToList();
            for (int i = 0; i < genericArguments.Count; i++)
            {
                if (typeMap.ContainsKey(genericArguments[i]))
                    genericArguments[i] = typeMap[genericArguments[i]];
            }
            return Expression.Call(
                node.Method.DeclaringType,
                node.Method.Name,
                genericArguments.ToArray(),
                args.ToArray());
        }
        private static object DynamicAverage(this IQueryable source, dynamic selector)
        {
            ParameterExpression newParamter = Expression.Parameter(source.ElementType, "x");
            var newBody = new StatisVisitor(newParamter).Visit(selector.Body);
            dynamic dynSelector = Expression.Lambda(newBody, newParamter);
            dynamic dynSource = source;

            return Queryable.Average(dynSource, dynSelector);
        }
        private static object DynamicMax(this IQueryable source, dynamic selector)
        {
            ParameterExpression newParamter = Expression.Parameter(source.ElementType, "x");
            var newBody = new StatisVisitor(newParamter).Visit(selector.Body);
            dynamic dynSelector = Expression.Lambda(newBody, newParamter);
            dynamic dynSource = source;

            return Queryable.Max(dynSource, dynSelector);
        }
        private static object DynamicMin(this IQueryable source, dynamic selector)
        {
            ParameterExpression newParamter = Expression.Parameter(source.ElementType, "x");
            var newBody = new StatisVisitor(newParamter).Visit(selector.Body);
            dynamic dynSelector = Expression.Lambda(newBody, newParamter);
            dynamic dynSource = source;

            return Queryable.Min(dynSource, dynSelector);
        }

        #endregion
    }
}
