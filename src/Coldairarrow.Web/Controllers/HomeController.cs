using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Business.Common;
using Coldairarrow.Util;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class HomeController : BaseMvcController
    {
        HomeBusiness _homeBus { get; } = new HomeBusiness();

        #region 视图功能

        public ActionResult Index()
        {
            return View();
        }

        [IgnoreLogin]
        public ActionResult Login()
        {
            if (Operator.Logged())
            {

                string loginUrl = Url.Content("~/");
                string script = $@"    
<html>
    <script>
        top.location.href = '{loginUrl}';
    </script>
</html>
";
                return Content(script);
            }

            return View();
        }

        public ActionResult Desktop()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        #endregion

        #region 获取数据



        #endregion

        #region 提交数据

        [IgnoreLogin]
        public ActionResult SubmitLogin(string userName, string password)
        {
            AjaxResult res = _homeBus.SubmitLogin(userName, password);

            return Content(res.ToJson());
        }

        /// <summary>
        /// 注销
        /// </summary>
        public ActionResult Logout()
        {
            Operator.Logout();

            return Success("注销成功！");
        }

        #endregion
    }
}