using Coldairarrow.Util;
using System.Web;

namespace Coldairarrow.Business.Base_SysManage
{
    public interface ICheckSignBusiness
    {
        AjaxResult IsSecurity(HttpContext context);
        string GetAppSecret(string appId);
    }
}