using Coldairarrow.DataRepository;
using System;
using System.IO;
using System.Linq;
using Coldairarrow.Util;
using Coldairarrow.Business.Base_SysManage;
using Coldairarrow.Entity.Base_SysManage;
using System.Collections.Generic;
using System.Linq.Dynamic;

namespace Coldairarrow.Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            Base_UserBusiness base_UserBusiness = new Base_UserBusiness();
            List<string> ids = new List<string>() { "Admin" };
            //var list = base_UserBusiness.GetIQueryable().Where("@0.Contains(outerIt.UserId)", ids).ToList();
            //List<Base_User> insertList = new List<Base_User>();
            //LoopHelper.Loop(100, index =>
            //{
            //    insertList.Add(new Base_User
            //    {
            //        Id = GuidHelper.GenerateKey(),
            //        UserId = GuidHelper.GenerateKey(),
            //        UserName = $"名字{index}",
            //        RealName = $"名字{index}"
            //    });
            //});
            //base_UserBusiness.Insert(insertList);

            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
