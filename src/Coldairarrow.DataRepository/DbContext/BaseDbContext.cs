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

        }
    }
}
