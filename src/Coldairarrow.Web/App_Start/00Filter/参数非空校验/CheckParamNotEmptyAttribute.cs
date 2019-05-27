using Coldairarrow.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    /// <summary>
    /// 参数非空校验
    /// </summary>
    public class CheckParamNotEmptyAttribute : FilterAttribute, IActionFilter
    {
        private List<string> _paramters { get; }
        public CheckParamNotEmptyAttribute(params string[] paramters)
        {
            _paramters = paramters.ToList();
        }

        /// <summary>
        /// Action执行之前执行
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var allParamters = HttpHelper.GetAllRequestParams(HttpContext.Current);
            var needParamters = _paramters.Where(x =>
            {
                if (!allParamters.ContainsKey(x))
                    return true;
                else
                    return allParamters[x].IsNullOrEmpty();
            }).ToList();
            if (needParamters.Count != 0)
            {
                AjaxResult res = new AjaxResult
                {
                    Success = false,
                    Msg = $"参数:{string.Join(",", needParamters)}不能为空！"
                };
                filterContext.Result = new ContentResult { Content = res.ToJson(), ContentEncoding = Encoding.UTF8,ContentType= "application/json;charset=utf-8" };
            }
        }

        /// <summary>
        /// Action执行完毕之后执行
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}