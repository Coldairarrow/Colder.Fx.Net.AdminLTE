using Coldairarrow.Util;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace Coldairarrow.DataRepository
{
    public static class DbModelFactory
    {
        #region 构造函数

        static DbModelFactory()
        {
            InitModelType();
        }

        #endregion

        #region 外部接口

        public static void AddObserver(IRepositoryDbContext observer)
        {
            _observers.Add(observer);
        }

        public static void RemoveObserver(IRepositoryDbContext observer)
        {
            _observers.Remove(observer);
        }

        /// <summary>
        /// 获取DbCompiledModel
        /// </summary>
        /// <param name="conStr">数据库连接名或字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        public static DbCompiledModel GetDbCompiledModel(string conStr, DatabaseType dbType)
        {
            string modelInfoId = GetCompiledModelIdentity(conStr, dbType);
            if (_dbCompiledModel.ContainsKey(modelInfoId))
                return _dbCompiledModel[modelInfoId].DbCompiledModel;
            else
            {
                var theModelInfo = BuildDbCompiledModelInfo(conStr, dbType);
                _dbCompiledModel[modelInfoId] = theModelInfo;
                return theModelInfo.DbCompiledModel;
            }
        }

        /// <summary>
        /// 获取模型
        /// </summary>
        /// <param name="type">原类型</param>
        /// <returns></returns>
        public static Type GetModel(Type type)
        {
            string modelName = type.Name;

            if (_modelTypeMap.ContainsKey(modelName))
                return _modelTypeMap[modelName];
            else
            {
                _modelTypeMap[modelName] = type;
                RefreshModel();

                return type;
            }
        }

        #endregion

        #region 私有成员

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
                _modelTypeMap[aType.Name] = aType;
            });
        }
        private static SynchronizedCollection<IRepositoryDbContext> _observers { get; } = new SynchronizedCollection<IRepositoryDbContext>();
        private static ConcurrentDictionary<string, Type> _modelTypeMap { get; } = new ConcurrentDictionary<string, Type>();
        private static ConcurrentDictionary<string, DbCompiledModelInfo> _dbCompiledModel { get; } = new ConcurrentDictionary<string, DbCompiledModelInfo>();
        private static DbCompiledModelInfo BuildDbCompiledModelInfo(string nameOrConStr, DatabaseType dbType)
        {
            lock (_buildCompiledModelLock)
            {
                DbConnection connection = DbProviderFactoryHelper.GetDbConnection(nameOrConStr, dbType);
                DbModelBuilder modelBuilder = new DbModelBuilder(DbModelBuilderVersion.Latest);
                modelBuilder.HasDefaultSchema(GetSchema());
                var entityMethod = typeof(DbModelBuilder).GetMethod("Entity");
                _modelTypeMap.Values.ToList().ForEach(aModel =>
                {
                    entityMethod.MakeGenericMethod(aModel).Invoke(modelBuilder, null);
                });
                var theModel = modelBuilder.Build(connection).Compile();
                DbCompiledModelInfo info = new DbCompiledModelInfo
                {
                    ConStr = connection.ConnectionString,
                    DatabaseType = dbType,
                    DbCompiledModel = theModel
                };

                return info;

                string GetSchema()
                {
                    switch (dbType)
                    {
                        case DatabaseType.SqlServer: return "dbo";
                        case DatabaseType.MySql: case DatabaseType.PostgreSql: return "public";
                        case DatabaseType.Oracle: return new OracleConnectionStringBuilder(connection.ConnectionString).UserID; ;
                        default: return "dbo";
                    }
                }
            }
        }
        private static string GetCompiledModelIdentity(string conStr, DatabaseType dbType)
        {
            return $"{dbType.ToString()}{conStr}";
        }
        private static object _buildCompiledModelLock { get; } = new object();
        private static void RefreshModel()
        {
            _dbCompiledModel.Values.ForEach(aModelInfo =>
            {
                aModelInfo.DbCompiledModel = BuildDbCompiledModelInfo(aModelInfo.ConStr, aModelInfo.DatabaseType).DbCompiledModel;
            });

            _observers.ForEach(x => x.RefreshDb());
        }

        #endregion

        #region 数据结构

        class DbCompiledModelInfo
        {
            public DbCompiledModel DbCompiledModel { get; set; }
            public string ConStr { get; set; }
            public DatabaseType DatabaseType { get; set; }
        }

        #endregion
    }
}
