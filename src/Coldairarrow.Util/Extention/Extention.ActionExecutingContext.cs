using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 拓展类
    /// </summary>
    public static partial class Extention
    {
        /// <summary>
        /// 获取所有AOP特性
        /// </summary>
        /// <param name="filterContext">AOP对象</param>
        /// <param name="inherit">是否继承父类</param>
        /// <returns></returns>
        public static List<Attribute> GetAllCustomAttributes(this ActionExecutingContext filterContext, bool inherit=true)
        {
            var actionAttrs = filterContext.ActionDescriptor.GetCustomAttributes(inherit).Select(x=>x as Attribute).ToList();
            var controllerAttrs = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(inherit).Select(x=>x as Attribute).ToList();

            return actionAttrs.Concat(controllerAttrs).ToList();
        }

        /// <summary>
        /// 是否包含某特性
        /// </summary>
        /// <typeparam name="T">特性类型</typeparam>
        /// <param name="filterContext">AOP对象</param>
        /// <returns></returns>
        public static bool ContainsAttribute<T>(this ActionExecutingContext filterContext) where T : Attribute
        {
            return filterContext.GetAllCustomAttributes().Any(x => x.GetType() == typeof(T));
        }
    }
}
