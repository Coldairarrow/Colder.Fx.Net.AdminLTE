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
            IRepository db = DbFactory.GetRepository();

            var q = db.GetIQueryable<Base_User>();
            var qWhere = db.GetIQueryable<Base_User>().Where(x => true);
            
            var expression = qWhere.Expression as MethodCallExpression;
            var arg1 = expression.Arguments[0] as MethodCallExpression;
            var obj = (arg1.Object as ConstantExpression).Value as IQueryable<Base_User>;
            //bool eq = q == obj;
            var list = obj.ToList();
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
