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
            //IRepository db1 = DbFactory.GetRepository("oracle", DatabaseType.Oracle);
            //IRepository db2 = DbFactory.GetRepository("oracle2", DatabaseType.Oracle);
            //var list = db1.GetList<Base_User>();
            //var list2 = db2.GetList<Base_User>();
            var db = DbFactory.GetRepository();
            db.HandleSqlLog = Console.WriteLine;
            string tableName = "Base_User1";
            var typeConfig = TypeBuilderHelper.GetConfig(typeof(Base_User));
            typeConfig.FullName = tableName;
            typeConfig.Attributes[0].ConstructorArgs[0] = tableName;
            var type = TypeBuilderHelper.BuildType(typeConfig);
            
            var list = db.GetIQueryable(type).CastToList<object>();
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
