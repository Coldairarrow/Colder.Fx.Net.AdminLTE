using System;
using System.Linq.Expressions;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 表达式树帮助类
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// 根据表达式树获取属性名
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            var name = "";
            if (expression.Body is UnaryExpression)
            {
                name = ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member.Name;
            }
            else if (expression.Body is MemberExpression)
            {
                name = ((MemberExpression)expression.Body).Member.Name;
            }
            else if (expression.Body is ParameterExpression)
            {
                name = ((ParameterExpression)expression.Body).Type.Name;
            }
            return name;
        }
    }
}
