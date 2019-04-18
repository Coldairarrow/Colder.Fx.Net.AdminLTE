using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 分库分表读写分离配置
    /// </summary>
    public class ShardingConfig
    {
        private SynchronizedCollection<AbstractDatabse> _absDb { get; } = new SynchronizedCollection<AbstractDatabse>();
        private SynchronizedCollection<AbstractTable> _absTable { get; } = new SynchronizedCollection<AbstractTable>();
        private SynchronizedCollection<LogicDatabase> _logicDb { get; } = new SynchronizedCollection<LogicDatabase>();
        private SynchronizedCollection<PhysicDatabase> _physicDb { get; } = new SynchronizedCollection<PhysicDatabase>();
        private SynchronizedCollection<PhysicTable> _physicTable { get; } = new SynchronizedCollection<PhysicTable>();

        public List<(string conString, DatabaseType dbType, string tableName)> GetTargetTables<T>(ReadWriteType opType, string absDbName = null)
        {
            string absTableName = typeof(T).Name;

            //获取抽象表
            AbstractTable absTable = null;
            if (absDbName.IsNullOrEmpty())
                absTable = _absTable.Where(x => x.AbsTableName == absTableName).FirstOrDefault();
            else
                absTable = _absTable.Where(x => x.AbsTableName == absTableName && x.AbsDbName == absDbName).Single();

            //获取抽象数据库
            AbstractDatabse absDb = _absDb.Where(x => x.AbsDbName == absTable.AbsDbName).Single();

            //获取逻辑数据库
            List<LogicDatabase> logicDbs = _logicDb.Where(x => x.AbsDbName == absDb.AbsDbName && x.ReadWriteType.HasFlag(opType)).ToList();
            int index = logicDbs.Count == 1 ? 0 : RandomHelper.Next(0, logicDbs.Count - 1);
            LogicDatabase logicDb = logicDbs[index];

            //获取物理表
            var tables = (from a in _physicDb
                          join b in _physicTable on a.PhysicDbName equals b.PhysicDbName
                          where a.LogicDbName == logicDb.LogicDbName && b.AbsTableName == absTableName
                          select new
                          {
                              a.ConString,
                              b.PhysicTableName
                          }
                        ).Select(x => (x.ConString, absDb.DbType, x.PhysicTableName))
                        .ToList();

            //读操作,需要获取所有表
            if (opType == ReadWriteType.Read)
                return tables;
            //写操作,获取一个表
            else
            {
                int indexTable = RandomHelper.Next(0, tables.Count - 1);
                return new List<(string conString, DatabaseType dbType, string tableName)> { tables[indexTable] };
            }
        }
    }

    public enum ReadWriteType
    {
        Read = 1,
        Write = 2,
        ReadAndWrite = 3
    }

    /// <summary>
    /// 抽象数据库
    /// 注：即将所有读库与写库视为同一个整体数据库
    /// </summary>
    public class AbstractDatabse
    {
        public string AbsDbName { get; set; }
        public DatabaseType DbType { get; set; }
    }

    /// <summary>
    /// 抽象表
    /// 注：属于抽象数据库
    /// </summary>
    public class AbstractTable
    {
        public string AbsTableName { get; set; }
        public string AbsDbName { get; set; }
        public Func<string> FindTable { get; set; }
    }

    /// <summary>
    /// 逻辑数据库
    /// 注：逻辑读库、逻辑写库、逻辑读写库，即将逻辑库中的所有表视为一个整体
    /// </summary>
    public class LogicDatabase
    {
        public string AbsDbName { get; set; }
        public string LogicDbName { get; set; }
        public ReadWriteType ReadWriteType { get; set; }
    }

    /// <summary>
    /// 物理数据库
    /// 注：即真正的数据库，属于逻辑数据库
    /// </summary>
    public class PhysicDatabase
    {
        public string PhysicDbName { get; set; }
        public string LogicDbName { get; set; }
        public string ConString { get; set; }
    }

    /// <summary>
    /// 物理表
    /// 注：属于物理数据库，一张物理表对应一张抽象表
    /// </summary>
    public class PhysicTable
    {
        public string PhysicTableName { get; set; }
        public string PhysicDbName { get; set; }
        public string AbsTableName { get; set; }
    }
}
