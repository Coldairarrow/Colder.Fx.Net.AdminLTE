using Coldairarrow.Util;
using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class CommonController : BaseController
    {
        public ActionResult ShowBigImg(string url)
        {
            ViewData["url"] = url;
            return View();
        }

        public ActionResult UploadImg(string fileName, string data)
        {
            var url = ImgHelper.GetImgUrl(data);

            var res = new
            {
                success = true,
                src = url
            };
            return JsonContent(res.ToJson());
        }
    }
}