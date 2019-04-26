using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Coldairarrow.DataRepository
{
    class BaseDbContext : DbContext
    {
        public BaseDbContext(DbConnection existingConnection, DbCompiledModel model)
            : base(existingConnection, model, true)
        {
            Configuration.UseDatabaseNullSemantics = true;
        }

        static BaseDbContext()
        {
            //数据库已手动构建，不需要自己生成初始化
            Database.SetInitializer<BaseDbContext>(null);
        }
    }
}
