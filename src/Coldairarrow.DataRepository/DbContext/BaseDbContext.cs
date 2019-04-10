using Coldairarrow.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Coldairarrow.DataRepository
{
    public class BaseDbContext : DbContext
    {
        #region 构造函数

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static BaseDbContext()
        {
            //数据库已手动构建，不需要自己生成初始化
            Database.SetInitializer<BaseDbContext>(null);

            //初始化模型类型
            InitModelType();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nameOrConStr">数据库连接名或连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">数据库实体命名空间,注意,该命名空间应该包含所有需要的数据库实体</param>
        public BaseDbContext(string nameOrConStr, DatabaseType dbType, string entityNamespace)
            : base(GetDbConnection(nameOrConStr, dbType), GetDbCompiledModel(nameOrConStr, dbType), true)
        {
            _dbType = dbType;
        }

        #endregion

        #region 公有成员

        public static bool NeedReloadDb(Type type)
        {
            string modelId = GetIdentity(type);
            if (_modelTypeMap.ContainsKey(modelId))
                return false;
            else
            {
                AddSet(type);
                return true;
            }
        }

        public override DbSet<TEntity> Set<TEntity>()
        {
            return Set(typeof(TEntity)).Cast<TEntity>();
        }

        public override DbSet Set(Type entityType)
        {
            string modelId = GetIdentity(entityType);
            var type = _modelTypeMap[modelId];

            return base.Set(type);
        }

        #endregion

        #region 私有成员

        private static ConcurrentDictionary<DatabaseType, DbCompiledModel> _dbCompiledModel { get; } = new ConcurrentDictionary<DatabaseType, DbCompiledModel>();
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<TheEntity>();
            //以下代码最终目的就是将所有需要的实体类调用上面的方法加入到DbContext中，成为其中的一部分
            modelBuilder.HasDefaultSchema(GetSchema());
            var entityMethod = typeof(DbModelBuilder).GetMethod("Entity");
            _modelTypes.ToList().ForEach(aModel =>
            {
                entityMethod.MakeGenericMethod(aModel).Invoke(modelBuilder, null);
            });

            string GetSchema()
            {
                switch (_dbType)
                {
                    case DatabaseType.SqlServer: return "dbo";
                    case DatabaseType.MySql: case DatabaseType.PostgreSql: return "public";
                    case DatabaseType.Oracle: return Database.Connection.Database;
                    default: return "dbo";
                }
            }
        }
        private DatabaseType _dbType { get; set; }
        private static DbConnection GetDbConnection(string conStr, DatabaseType dbType)
        {
            if (conStr.IsNullOrEmpty())
                conStr = GlobalSwitch.DefaultDbConName;
            DbConnection dbConnection = DbProviderFactoryHelper.GetDbConnection(dbType);
            dbConnection.ConnectionString = DbProviderFactoryHelper.GetConStr(conStr);

            return dbConnection;
        }
        private static DbCompiledModel GetDbCompiledModel(string nameOrConStr, DatabaseType dbType)
        {
            if (_dbCompiledModel.ContainsKey(dbType))
                return _dbCompiledModel[dbType];
            else
            {
                DbConnection connection = GetDbConnection(nameOrConStr, dbType);
                DbModelBuilder modelBuilder = new DbModelBuilder(DbModelBuilderVersion.Latest);
                modelBuilder.HasDefaultSchema(GetSchema());
                var entityMethod = typeof(DbModelBuilder).GetMethod("Entity");
                _modelTypes.ToList().ForEach(aModel =>
                {
                    entityMethod.MakeGenericMethod(aModel).Invoke(modelBuilder, null);
                });

                var theModel = modelBuilder.Build(connection).Compile();

                _dbCompiledModel[dbType] = theModel;

                return theModel;

                string GetSchema()
                {
                    switch (dbType)
                    {
                        case DatabaseType.SqlServer: return "dbo";
                        case DatabaseType.MySql: case DatabaseType.PostgreSql: return "public";
                        case DatabaseType.Oracle: return connection.Database;
                        default: return "dbo";
                    }
                }
            }
        }
        private static DbCompiledModel BuildDbCompiledModel(string nameOrConStr, DatabaseType dbType)
        {
            DbConnection connection = GetDbConnection(nameOrConStr, dbType);
            DbModelBuilder modelBuilder = new DbModelBuilder(DbModelBuilderVersion.Latest);
            modelBuilder.HasDefaultSchema(GetSchema());
            var entityMethod = typeof(DbModelBuilder).GetMethod("Entity");
            _modelTypes.ToList().ForEach(aModel =>
            {
                entityMethod.MakeGenericMethod(aModel).Invoke(modelBuilder, null);
            });

            var theModel = modelBuilder.Build(connection).Compile();

            return theModel;

            string GetSchema()
            {
                switch (dbType)
                {
                    case DatabaseType.SqlServer: return "dbo";
                    case DatabaseType.MySql: case DatabaseType.PostgreSql: return "public";
                    case DatabaseType.Oracle: return connection.Database;
                    default: return "dbo";
                }
            }
        }
        private static void InitModelType()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Union(new Assembly[] { Assembly.Load("Coldairarrow.Entity") });
            List<Type> allTypes = new List<Type>();
            assemblies.ForEach(aAssembly =>
            {
                allTypes.AddRange(aAssembly.GetTypes());
            });
            List<Type> types = allTypes
                .Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null && x.FullName.Contains(GlobalSwitch.DefaultEntityNamespace))
                .ToList();

            types.ForEach(aType =>
            {
                _modelTypes.Add(aType);
                _modelTypeMap[GetIdentity(aType)] = aType;
            });
        }
        private static SynchronizedCollection<Type> _modelTypes { get; } = new SynchronizedCollection<Type>();
        private static ConcurrentDictionary<string, Type> _modelTypeMap { get; } = new ConcurrentDictionary<string, Type>();
        private static void AddSet(Type type)
        {
            var existsModel = _modelTypes.Where(x => x.Name == type.Name).FirstOrDefault();
            if (existsModel != null)
            {
                _modelTypes.Remove(existsModel);
                _modelTypeMap.TryRemove(GetIdentity(existsModel), out Type type1);
            }

            _modelTypes.Add(type);
            _modelTypeMap[GetIdentity(type)] = type;
            EntityModelCacheKey.ChangeCache();
        }
        private static string GetIdentity(Type type)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(type.Name);
            type.GetProperties().OrderBy(x => x.Name).ForEach(aProperty =>
            {
                builder.Append($"{aProperty.Name}{aProperty.PropertyType.FullName}");
            });

            return builder.ToString().ToMD5String();
        }
        private static void RefreshModel()
        {
            var dbTypes = _dbCompiledModel.Keys.ToList();
            dbTypes.ForEach(aDbType =>
            {

            });
        }

        #endregion
    }

    public class EntityModelCacheKey : IDbModelCacheKey
    {
        public static void ChangeCache()
        {
            _hashCode = Guid.NewGuid().ToString().GetHashCode();
        }
        private static int _hashCode { get; set; } = Guid.NewGuid().ToString().GetHashCode();
        public override bool Equals(object other)
        {
            if (other == null)
                return false;
            return other.GetHashCode() == _hashCode;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }

    public class EntityFrameworkConfiguration : DbConfiguration
    {
        public EntityFrameworkConfiguration()
        {
            SetModelCacheKey(ctx => new EntityModelCacheKey());
        }
    }
}
