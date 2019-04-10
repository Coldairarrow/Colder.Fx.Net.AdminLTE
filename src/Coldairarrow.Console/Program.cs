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

namespace Coldairarrow.Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            IRepository db = DbFactory.GetRepository();
            IRepository db2 = DbFactory.GetRepository();
            db2.HandleSqlLog = Console.WriteLine;

            var list = db.GetIQueryable<Base_User>().ChangeDbContext(db2.GetDbContext()).Where(x=>true).OrderBy(x => x.RealName);
            var expression = list.Expression;
            
            Console.WriteLine(ExpressionHelper.GetPropertyName<Base_UnitTest>(x => x.Id));
            //var list = db.GetIQueryable<Base_User>().ToList();
            //var list2 = db.GetIQueryable<Base_User>().ChangeSource(db.GetIQueryable<Base_User1>()).ToList();
            //var q = new List<string> { "a", "c", "b" }.AsQueryable().OrderBy(x => x);
            //var q2 = new List<string> { "b", "a", "d" }.AsQueryable();
            //var list1 = q2.ToList();
            //var qNew = q.ChangeSource(q2).ToList();
            //q2.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0].SetValue(q2, q.Expression);
            //var list = q2.ToList();
            //var expression = q2.Expression;
            //var list2 = q2.OrderBy(x=>x).ToList();
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
