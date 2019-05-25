using Coldairarrow.Util;
using System.Web;

namespace Coldairarrow.Business.Base_SysManage
{
    public interface ICheckSignBusiness
    {
        /// <summary>
        /// 判断是否有权限操作接口
        /// </summary>
        /// <returns></returns>
        AjaxResult IsSecurity(HttpContext context);

        /// <summary>
        /// 获取应用密钥
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <returns></returns>
        string GetAppSecret(string appId);
    }
}

