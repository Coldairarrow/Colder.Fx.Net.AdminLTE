using Coldairarrow.Business;
using Coldairarrow.DotNettyRPC;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public interface IHello
    {
        string SayHello(string msg);
    }
    public class Hello : IHello
    {
        public string SayHello(string msg)
        {
            return msg;
        }
    }

    public class TestController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [CheckParamNotEmpty("aa")]
        public ActionResult Test()
        {


            return Success();
        }
    }
}