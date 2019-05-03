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
            var db = DbFactory.GetRepository().ToSharding();
            //Base_User newUser = new Base_User
            //{
            //    Id = GuidHelper.GenerateKey(),
            //    UserName = GuidHelper.GenerateKey(),
            //    RealName = GuidHelper.GenerateKey()
            //};
            //db.Insert(newUser);
            var data = db.GetIShardingQueryable<Base_User>()
               .Where(x => x.Id == "039ED906138B9-3B1D9CB1-147E-425E-8B33-FDE792B89411")
               .FirstOrDefault();
            data.RealName = "修改";
            db.Update(data);

            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}