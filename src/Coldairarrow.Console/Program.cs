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
            IRepository db1 = DbFactory.GetRepository("oracle",DatabaseType.Oracle);
            IRepository db2 = DbFactory.GetRepository("oracle2", DatabaseType.Oracle);
            var list = db1.GetList<Base_User>();
            var list2 = db2.GetList<Base_User>();
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
