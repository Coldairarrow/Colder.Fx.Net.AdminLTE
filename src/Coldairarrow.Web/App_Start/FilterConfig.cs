using System.Web.Mvc;

namespace Coldairarrow.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandlerGlobalError());
        }
    }
}
