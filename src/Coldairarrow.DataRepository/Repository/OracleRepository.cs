using Coldairarrow.Util;
using System;

namespace Coldairarrow.DataRepository
{
    public class OracleRepository : DbRepository, IRepository
    {
        #region 构造函数

        public OracleRepository()
            : base(null, DatabaseType.Oracle, null)
        {
        }

        public OracleRepository(string conStr)
            : base(conStr, DatabaseType.Oracle, null)
        {
        }

        public OracleRepository(string conStr, string entityNamespace)
            : base(conStr, DatabaseType.Oracle, entityNamespace)
        {
        }

        #endregion

        #region 插入数据

        #endregion
    }
}
