using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 分库分表读写分离配置
    /// </summary>
    public class ShardingConfig
    {
        public List<(string conString, DatabaseType dbType, string tableName)> GetTargetTables<T>()
        {
            return null;
        }


    }

    public enum ReadWriteType
    {
        Read = 1,
        Write = 2,
        ReadAndWrite = 3
    }

    public class DatabaseConfig
    {
        public string ConString { get; set; }
        public DatabaseType DatabaseType { get; set; }
        public ReadWriteType ReadWriteType { get; set; }

    }

    public class LogicTableConfig
    {
        public string LogicTableName { get; set; }
        public List<string> PhysicTableName { get; set; }
        public Func<string> FindTable { get; set; }
        public bool AutoCreateTable { get; set; }
    }
}
