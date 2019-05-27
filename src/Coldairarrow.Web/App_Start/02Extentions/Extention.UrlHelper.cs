using Coldairarrow.Util;
using System.IO;

namespace System.Web.Mvc
{
    public static partial class Extention
    {
        /// <summary>
        /// 获取最新的s文件或css文件
        /// 注：解决缓存问题，只有文件修改后才会获取最新版
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="scriptVirtualPath"></param>
        /// <returns></returns>
        public static string Scrpit(this UrlHelper helper, string scriptVirtualPath)
        {
            int version = 0;
            if(GlobalSwitch.RunModel== RunModel.LocalTest)
            {
                version = Guid.NewGuid().GetHashCode();
            }
            else
            {
                string filePath = helper.RequestContext.HttpContext.Server.MapPath(scriptVirtualPath);
                FileInfo fileInfo = new FileInfo(filePath);
                version = fileInfo.LastWriteTime.GetHashCode();
            }
            return helper.Content($"{scriptVirtualPath}?_v={version}");
        }
    }
}