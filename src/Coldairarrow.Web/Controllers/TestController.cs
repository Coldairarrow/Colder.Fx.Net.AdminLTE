using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class TestController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [CheckSign]
        public ActionResult RequestTest()
        {
            return Content("aa");
        }

        public ActionResult RequestDemo()
        {
            return View();
        }
    }
}