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

            Dictionary<string, int> rateDic = new Dictionary<string, int>();
            ConsistentHashExpand<string> consistentHash = new ConsistentHashExpand<string>();
            List<string> dataList = new List<string>(1000000);
            int count = 1000000;
            LoopHelper.Loop(count, () =>
            {
                dataList.Add(Guid.NewGuid().ToString());
            });
            
            List<string> node1 = new List<string> { "a", "b", "c" };
            List<string> node2 = new List<string> { "a", "b", "c", "d" };

            consistentHash.Init(node1);
            rateDic = new Dictionary<string, int>
            {
                { "a",0},
                { "b",0},
                { "c",0}
            };

            dataList.ForEach(aData =>
            {
                rateDic[consistentHash.GetNode(aData)]++;
            });
            Console.WriteLine(string.Join(",", rateDic.Select(x => $"{(double)x.Value/ count*100}%")));

            consistentHash.Init(node2);
            rateDic = new Dictionary<string, int>
            {
                { "a",0},
                { "b",0},
                { "c",0},
                { "d",0}
            };
            dataList.ForEach(aData =>
            {
                rateDic[consistentHash.GetNode(aData)]++;
            });
            Console.WriteLine(string.Join(",", rateDic.Select(x => $"{(double)x.Value / count * 100}%")));
            long.MaxValue
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}