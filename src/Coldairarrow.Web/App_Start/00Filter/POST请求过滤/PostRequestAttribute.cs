using Coldairarrow.Util;
using System.Text;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class PostRequestAttribute : FilterAttribute, IActionFilter
    {
        /// <summary>
        /// Action执行之前执行
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.HttpMethod.ToLower() != "post")
            {
                ErrorResult res = new ErrorResult
                {
                    Msg = "仅支持POST方法"
                };
                filterContext.Result = new ContentResult { Content = res.ToJson(), ContentEncoding = Encoding.UTF8 };
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