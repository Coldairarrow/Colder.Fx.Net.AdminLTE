using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Coldairarrow.DataRepository
{
    public interface IRepositoryDbContext : IDisposable
    {
        DbContext GetDbContext();
        Action<string> HandleSqlLog { get; set; }
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        DbSet Set(Type entityType);
        DbEntityEntry Entry(object entity);
        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        int SaveChanges();
        Database Database { get; }
        Type CheckEntityType(Type entityType);
        void UseTransaction(DbTransaction transaction);
    }
}
