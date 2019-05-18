using Coldairarrow.DataRepository;
using System;
using System.IO;
using System.Linq;
using Coldairarrow.Util;
using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Entity;
using Coldairarrow.Entity.Base_SysManage;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Collections;
using System.Reflection;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Console1
{
    class Program
    {
        static void ShardingTest()
        {
            var db = DbFactory.GetRepository();
            Stopwatch watch = new Stopwatch();
            var q = db.GetIQueryable<Base_SysLog>()
                .Where(x => x.LogContent.Contains("8C7719FF-5038-4C0A-9D0F-83830F0929E9"))
                .OrderByDescending(x => x.OpTime)
                .Skip(0)
                .Take(30);
            
            q.ToList();
            q.ToSharding().ToList();
            watch.Restart();
            var list1 = q.ToList();
            watch.Stop();
            Console.WriteLine($"未分表耗时:{watch.ElapsedMilliseconds}ms");
            watch.Restart();
            var list2 = q.ToSharding().ToList();
            watch.Stop();
            Console.WriteLine($"分表后耗时:{watch.ElapsedMilliseconds}ms");
        }

        static void Main(string[] args)
        {
            //ShardingTest();

            ShardingConfigBootstrapper.Bootstrap()
    //添加数据源
    .AddDataSource("BaseDb", DatabaseType.SqlServer, dbBuilder =>
    {
                    //添加物理数据库
                    dbBuilder.AddPhsicDb("BaseDb", ReadWriteType.ReadAndWrite);
    })
    //添加抽象数据库
    .AddAbsDb("BaseDb", absTableBuilder =>
    {
                    //添加抽象数据表
                    absTableBuilder.AddAbsTable("Base_UnitTest", tableBuilder =>
        {
                        //添加物理数据表
                        tableBuilder.AddPhsicTable("Base_UnitTest_0", "BaseDb");
            tableBuilder.AddPhsicTable("Base_UnitTest_1", "BaseDb");
            tableBuilder.AddPhsicTable("Base_UnitTest_2", "BaseDb");
        }, new ModShardingRule("Base_UnitTest", "Id", 3));
    });
            Base_UnitTest _newData = new Base_UnitTest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = "Admin",
                UserName = "超级管理员",
                Age = 22
            };
            var db = DbFactory.GetRepository().ToSharding();
            db.Insert(_newData);

            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}