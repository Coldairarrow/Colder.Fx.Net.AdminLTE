using AutoMapper;
using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.DataRepository;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Coldairarrow.Web
{
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// 程序启动时执行
        /// 注：重新编译后执行
        /// </summary>
        protected void Application_Start()
        {
            //注册路由
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //注册全局异常捕捉器
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            InitAutoMapper();
            InitEF();
        }

        /// <summary>
        /// 初始化AutoMapper
        /// </summary>
        private void InitAutoMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Base_User, Base_UserModel>();
            });
        }

        /// <summary>
        /// EF预热
        /// </summary>
        private void InitEF()
        {
            Task.Run(() =>
            {
                var db = DbFactory.GetRepository();
                db.GetIQueryable<Base_User>().ToList();
            });
        }
    }
}
