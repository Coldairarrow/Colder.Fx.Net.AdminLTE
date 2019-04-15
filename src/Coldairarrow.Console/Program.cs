using Coldairarrow.DataRepository;
using System;
using System.IO;
using System.Linq;
using Coldairarrow.Util;
using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Entity.Base_SysManage;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;

namespace Coldairarrow.Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            IRepository db = DbFactory.GetRepository("oracle", DatabaseType.Oracle);
            IRepository db2 = DbFactory.GetRepository("oracle", DatabaseType.Oracle);
            var q = db.GetIQueryable<Base_User>()/*OrderBy(x=>x.Id)*/.Where(x=>x.RealName.Contains("aaa"));
            var type = ((System.Data.Entity.Infrastructure.DbQuery)q).ElementType;
            //var qWhere = db.GetIQueryable<Base_User>().Where("True");

            //var expression = qWhere.Expression as MethodCallExpression;
            //var arg1 = expression.Arguments[0] as MethodCallExpression;
            //var obj = (arg1.Object as ConstantExpression).Value as IQueryable<Base_User>;
            //var list = obj.ToList();
            var list = q.ChangeSource(db.GetIQueryable<Base_User1>());
            //var list = q.ChangeDbContext(db2.GetDbContext()).ToList();
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
