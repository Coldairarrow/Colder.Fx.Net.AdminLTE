using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.DataRepository;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System.Linq;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class TestController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        //[CheckSign]
        public ActionResult RequestTest()
        {
            var bus = AutofacHelper.GetService<IBase_UserBusiness>();
            var db = DbFactory.GetRepository();
            Base_User data = new Base_User
            {
                Id = IdHelper.GetId(),
                UserName = IdHelper.GetId()
            };
            db.Insert(data);
            db.Update(data);
            db.GetIQueryable<Base_User>().FirstOrDefault();
            db.Delete(data);
            //db.Dispose();

            return Success("");
        }

        public ActionResult RequestDemo()
        {
            return View();
        }
    }
}