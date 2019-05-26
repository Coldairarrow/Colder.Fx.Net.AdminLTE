using Coldairarrow.Business;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class TestController : BaseController
    {
        public TestController(ITestBusiness testBus)
        {
            _testBus = testBus;
        }
        private ITestBusiness _testBus { get; }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetValue()
        {
            return Content(_testBus.GetValue());
        }
    }
}