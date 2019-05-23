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
using System.Collections.Concurrent;

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
            ShardingConfigBootstrapper
                .Bootstrap()
                //添加数据源
                .AddDataSource("BaseDb", DatabaseType.SqlServer, dbBuilder =>
                {
                    //添加物理数据库
                    dbBuilder.AddPhsicDb("BaseDb", ReadWriteType.Read);
                    //添加物理数据库
                    dbBuilder.AddPhsicDb("BaseDb1", ReadWriteType.ReadAndWrite);
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
            List<Base_UnitTest> insertList = new List<Base_UnitTest>();
            for (int i = 1; i <= 100; i++)
            {
                Base_UnitTest newData = new Base_UnitTest
                {
                    Id = Guid.NewGuid().ToString(),
                    Age = i,
                    UserId = "Admin" + i,
                    UserName = "超级管理员" + i
                };
                insertList.Add(newData);
            }

            var db = DbFactory.GetShardingRepository();
            //db.Insert(insertList);

            var dbList = db.GetIShardingQueryable<Base_UnitTest>().ToList();

            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}