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
            var type2 = TypeBuilderHelper.BuildType(typeConfig);
            var properties = type.GetProperties();
            Console.WriteLine(type == type2);
            Base_User base_User = new Base_User
            {
                Id = GuidHelper.GenerateKey(),
                UserId = GuidHelper.GenerateKey(),
                UserName= GuidHelper.GenerateKey()
            };
            Base_User base_User2 = new Base_User
            {
                Id = GuidHelper.GenerateKey(),
                UserId = GuidHelper.GenerateKey(),
                UserName = GuidHelper.GenerateKey()
            };

            db.Insert(base_User.ToJson().ToObject(type));
            db.Insert(base_User2.ChangeType(type2));
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
