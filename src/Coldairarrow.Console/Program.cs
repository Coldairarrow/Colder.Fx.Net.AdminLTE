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
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Coldairarrow.Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            IRepository db = DbFactory.GetRepository();
            string keyword = "";
            var q = db.GetIQueryable<Base_User>().Where(x=>x.RealName.Contains(keyword)).GetPagination(new Pagination()).RemoveSkip().RemoveTake();
            string oldTableName = typeof(Base_User).Name;
            string targetTableName = typeof(Base_User1).Name;
            var sql = q.ToSQL();
            string sqlStr = sql.sql.Replace(oldTableName, targetTableName);
            List<DbParameter> _paramters = sql.parameters.Select(x =>
            {
                var aParam = DbProviderFactoryHelper.GetDbParameter(DatabaseType.SqlServer);
                aParam.ParameterName = x.Name;
                aParam.Value = x.Value;
                return aParam;
            }).ToList();
            var list = db.GetListBySql<Base_User>(sqlStr, _paramters);
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
