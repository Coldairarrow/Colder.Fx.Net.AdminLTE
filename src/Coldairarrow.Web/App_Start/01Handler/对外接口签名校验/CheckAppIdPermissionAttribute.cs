using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Util;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    /// <summary>
    /// 校验AppId接口权限
    /// </summary>
    public class CheckAppIdPermissionAttribute : FilterAttribute, IActionFilter
    {
        /// <summary>
        /// Action执行之前执行
        /// </summary>
        /// <param name="filterContext">过滤器上下文</param>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //若为本地测试，则不需要校验
            if (GlobalSwitch.RunModel == RunModel.LocalTest)
            {
                return;
            }
            AjaxResult res = new AjaxResult();
            //判断是否需要校验
            bool needCheck = filterContext.ContainsAttribute<CheckAppIdPermissionAttribute>() && !filterContext.ContainsAttribute<IgnoreAppIdPermissionAttribute>();
            if (!needCheck)
                return;

            var allRequestParams = HttpHelper.GetAllRequestParams(filterContext.HttpContext.ApplicationInstance.Context);
            if(!allRequestParams.ContainsKey("appId"))
            {
                res.Success = false;
                res.Msg = "缺少appId参数！";
                filterContext.Result = new ContentResult { Content = res.ToJson(), ContentEncoding = Encoding.UTF8 };
            }
            string appId = allRequestParams["appId"]?.ToString();
            var allUrlPermissions = UrlPermissionManage.GetAllUrlPermissions();
            string requestUrl = filterContext.HttpContext.Request.Url.ToString().ToLower();
            var thePermission = allUrlPermissions.Where(x => requestUrl.Contains(x.Url.ToLower())).FirstOrDefault();
            if (thePermission == null)
                return;
            string needPermission = thePermission.PermissionValue;
            bool hasPermission = PermissionManage.GetAppIdPermissionValues(appId).Any(x => x.ToLower() == needPermission.ToLower());
            if (hasPermission)
                return;
            else
            {
                res.Success = false;
                res.Msg = "权限不足！访问失败！";
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