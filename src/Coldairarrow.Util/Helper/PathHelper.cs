using System.Web;
using System.Web.Mvc;

namespace Coldairarrow.Util
{
    public static class PathHelper
    {
        /// <summary>
        /// 获取Url
        /// </summary>
        /// <param name="virtualUrl">虚拟Url</param>
        /// <returns></returns>
        public static string GetUrl(string virtualUrl)
        {
            if (!virtualUrl.IsNullOrEmpty())
            {
                UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                return urlHelper.Content(virtualUrl);
            }
            else
                return null;
        }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="virtualPath">虚拟路径</param>
        /// <returns></returns>
        public static string GetAbsolutePath(string virtualPath)
        {
            return HttpContext.Current.Server.MapPath(virtualPath);
        }
    }
}
