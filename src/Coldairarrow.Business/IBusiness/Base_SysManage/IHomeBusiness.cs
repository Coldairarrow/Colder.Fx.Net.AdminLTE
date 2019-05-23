using Coldairarrow.Util;

namespace Coldairarrow.Business
{
    public interface IHomeBusiness : IDependency
    {
        AjaxResult SubmitLogin(string userName, string password);
    }
}
