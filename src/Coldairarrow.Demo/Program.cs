//using Coldairarrow.DataRepository;
//using Coldairarrow.Entity.Base_SysManage;
//using Coldairarrow.Entity.DevManage;
//using Coldairarrow.Util;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;

//namespace Coldairarrow.Demo
//{
//    public class Dev_ProjectDTO : Dev_Project
//    {
//        /// <summary>
//        /// 项目经理
//        /// </summary>
//        public string ProjectManager { get; set; }

//        /// <summary>
//        /// 项目类型名
//        /// </summary>
//        public string ProjectTypeName { get; set; }
//    }
//    class Program
//    {
//        /// <summary>
//        /// 查询示例
//        /// 使用EF + LINQ + Expression能够满足绝大多数查询需求
//        /// 其中核心需要使用IRepository接口中的GetIQueryable<T>
//        /// 熟练掌握IQueryable<T>的使用，从最基础的查询到进阶的联表查询，到最后高阶的数据权限过滤
//        /// IQueryable<T>扮演着核心的角色
//        /// 注意Business类中的Service就是IRepository接口，通过GetIQueryable<T>能够获取数据源
//        /// </summary>
//        public static void SearchDemo()
//        {
//            Pagination pagination = new Pagination();

//            var list = GetProjectList(null, null, null, pagination);

//            Console.WriteLine(list.ToJson());
//        }

//        public static List<Dev_ProjectDTO> GetProjectList(string projectName, string projectTypeName, string projectManager, Pagination pagination)
//        {
//            var db = DbFactory.GetRepository();

//            var where = LinqHelper.True<Dev_ProjectDTO>();
//            Expression<Func<Dev_Project, Dev_ProjectType, Base_User, Dev_ProjectDTO>> select = (a, b, c) => new Dev_ProjectDTO
//            {
//                ProjectTypeName = b.ProjectTypeName,
//                ProjectManager = c.RealName
//            };

//            select = select.BuildExtendSelectExpre();

//            var q = from a in db.GetIQueryable<Dev_Project>().AsExpandable()
//                    join b in db.GetIQueryable<Dev_ProjectType>() on a.ProjectTypeId equals b.ProjectTypeId into ab
//                    from b in ab.DefaultIfEmpty()
//                    join c in db.GetIQueryable<Base_User>() on a.ProjectManagerId equals c.UserId into ac
//                    from c in ac.DefaultIfEmpty()
//                    select @select.Invoke(a, b, c);

//            if (!projectName.IsNullOrEmpty())
//                where = where.And(x => x.ProjectName.Contains(projectName));
//            if (!projectTypeName.IsNullOrEmpty())
//                where = where.And(x => x.ProjectTypeName.Contains(projectTypeName));
//            if (!projectManager.IsNullOrEmpty())
//                where = where.And(x => x.ProjectManager.Contains(projectManager));

//            return q.Where(where).GetPagination(pagination).ToList();
//        }

//        static void Main(string[] args)
//        {
//            SearchDemo();

//            Console.WriteLine("完成");
//            Console.ReadLine();
//        }
//    }
//}
