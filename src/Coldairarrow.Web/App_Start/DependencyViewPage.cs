using Coldairarrow.Business;
using Coldairarrow.Business.Base_SysManage;

namespace System.Web.Mvc
{
    public abstract class DependencyViewPage : WebViewPage
    {
        public IPermissionManage PermissionManage { get; set; }
        public ISystemMenuManage SystemMenuManage { get; set; }
        public IBase_UserBusiness SysUserBus { get; set; }
        public IOperator Operator { get; set; }
    }
}