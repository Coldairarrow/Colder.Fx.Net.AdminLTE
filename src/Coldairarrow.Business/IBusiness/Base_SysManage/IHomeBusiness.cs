using Coldairarrow.Util;

namespace Coldairarrow.Business
{
    public interface IHomeBusiness
    {
        AjaxResult SubmitLogin(string userName, string password);
    }
}
