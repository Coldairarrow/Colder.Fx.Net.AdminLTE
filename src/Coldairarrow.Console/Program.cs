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

namespace Coldairarrow.Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            IRepository db = DbFactory.GetRepository();

            var list = db.GetIQueryable<Base_User>().OrderBy(x => x.Id).ToList();
            var list2 = db.GetIQueryable<Base_User>().OrderBy(x => x.Id).RemoveOrderBy().ToList();

            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
