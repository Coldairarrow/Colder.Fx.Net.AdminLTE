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
using System.Threading.Tasks;

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
            List<Task> tasks = new List<Task>();
            LoopHelper.Loop(4, () =>
            {
                tasks.Add(Task.Run(() =>
                {
                    LoopHelper.Loop(1000000, () =>
                    {
                        var db = DbFactory.GetRepository();
                        db.Insert(new Base_UnitTest
                        {
                            Id = IdHelper.GetId(),
                            UserId= IdHelper.GetId(),
                            UserName= IdHelper.GetId()
                        });
                    });
                }));
            });
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}