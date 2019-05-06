using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coldairarrow.Util.Snowflake
{
    public struct SnowflakeId
    {
        static SnowflakeId()
        {
            _idWorker = new IdWorker(1, 1);
        }
        private static IdWorker _idWorker { get; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
