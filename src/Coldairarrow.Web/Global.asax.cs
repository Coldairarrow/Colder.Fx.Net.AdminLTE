﻿using Autofac;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Mvc;
using AutoMapper;
using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.DataRepository;
using Coldairarrow.Entity.Base_SysManage;
using Coldairarrow.Util;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
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

            InitAutofac();

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
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Base_User, Base_UserDTO>();
                cfg.CreateMap<Base_SysRole, Base_SysRoleDTO>();
            });

            AutoMapperHelper.Mapper = configuration.CreateMapper();
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

        private void InitAutofac()
        {
            var builder = new ContainerBuilder();

            var baseType = typeof(IDependency);
            var baseTypeCircle = typeof(ICircleDependency);

            //Coldairarrow相关程序集
            var assemblys = BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                .Where(x => x.FullName.Contains("Coldairarrow")).ToList();

            //自动注入IDependency接口,支持AOP,生命周期为InstancePerDependency
            builder.RegisterAssemblyTypes(assemblys.ToArray())
                .Where(x => baseType.IsAssignableFrom(x) && x != baseType)
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .InstancePerDependency()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(Interceptor));

            //自动注入ICircleDependency接口,循环依赖注入,不支持AOP,生命周期为InstancePerLifetimeScope
            builder.RegisterAssemblyTypes(assemblys.ToArray())
                .Where(x => baseTypeCircle.IsAssignableFrom(x) && x != baseTypeCircle)
                .AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .InstancePerLifetimeScope();

            //注册Controller
            builder.RegisterControllers(assemblys.ToArray())
                .PropertiesAutowired();

            //注册Filter
            builder.RegisterFilterProvider();

            //注册View
            builder.RegisterSource(new ViewRegistrationSource());

            //AOP
            builder.RegisterType<Interceptor>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            AutofacHelper.Container = container;
        }
    }
}
