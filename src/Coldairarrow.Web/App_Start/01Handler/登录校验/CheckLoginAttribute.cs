using Coldairarrow.Business;
using Coldairarrow.Util;
using System;
using System.Text;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    /// <summary>
    /// 校验登录
    /// </summary>
    public class CheckLoginAttribute : FilterAttribute, IActionFilter
    {
        public IOperator _operator { get; set; }
        public IBusHelper _busHelper { get; set; }

        /// <summary>
        /// Action执行之前执行
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            try
            {
                //若为本地测试，则不需要登录
                if (GlobalSwitch.RunModel == RunModel.LocalTest)
                {
                    return;
                }
                //判断是否需要登录
                bool needLogin = filterContext.ContainsAttribute<CheckLoginAttribute>() && !filterContext.ContainsAttribute<IgnoreLoginAttribute>();

                //转到登录
                if (needLogin && !_operator.Logged())
                {
                    RedirectToLogin();
                }
                else
                    return;
            }
            catch (Exception ex)
            {
                _busHelper.HandleException(ex);
                RedirectToLogin();
            }

            void RedirectToLogin()
            {
                if (request.IsAjaxRequest())
                {
                    filterContext.Result = new ContentResult
                    {
                        Content = new AjaxResult { Success = false, ErrorCode = 1, Msg = "未登录" }.ToJson(),
                        ContentEncoding = Encoding.UTF8,
                        ContentType = "application/json"
                    };
                }
                else
                {
                    UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
                    string loginUrl = urlHelper.Content("~/Home/Login");
                    string script = $@"    
<html>
    <script>
        top.location.href = '{loginUrl}';
    </script>
</html>
";
                    filterContext.Result = new ContentResult { Content = script, ContentType = "text/html", ContentEncoding = Encoding.UTF8 };
                }
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