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
    public static class DbModelFactory
    {
        #region 构造函数

        static DbModelFactory()
        {
            InitModelType();
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 获取DbCompiledModel
        /// </summary>
        /// <param name="nameOrConStr">数据库连接名或字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <returns></returns>
        public static DbCompiledModel GetDbCompiledModel(string nameOrConStr, DatabaseType dbType)
        {
            if (_dbCompiledModel.ContainsKey(dbType))
                return _dbCompiledModel[dbType];
            else
            {
                var theModel = BuildDbCompiledModel(nameOrConStr, dbType);

                _dbCompiledModel[dbType] = theModel;

                return theModel;
            }
        }

        public static (bool needRefresh, Type model) GetModel(Type type)
        {
            bool needRefresh = false;
            Type targetType = null;
            if (_modelTypes.Contains(type))
                targetType = _modelTypes.Where(x => x == type).FirstOrDefault();
            else
            {
                string modelId = GetIdentity(type);
                if (_modelTypeMap.ContainsKey(modelId))
                    targetType = _modelTypeMap[modelId];
                else
                {
                    _modelTypes.Add(type);
                    _modelTypeMap[modelId] = type;
                    targetType = type;
                    needRefresh = true;
                }
            }

            return (needRefresh, targetType);
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
                _modelTypes.Add(aType);
                _modelTypeMap[GetIdentity(aType)] = aType;
            });
        }
        private static ConcurrentDictionary<string, Type> _modelTypeMap { get; } = new ConcurrentDictionary<string, Type>();
        private static SynchronizedCollection<Type> _modelTypes { get; } = new SynchronizedCollection<Type>();
        private static ConcurrentDictionary<DatabaseType, DbCompiledModel> _dbCompiledModel { get; } = new ConcurrentDictionary<DatabaseType, DbCompiledModel>();
        private static DbConnection GetDbConnection(string conStr, DatabaseType dbType)
        {
            if (conStr.IsNullOrEmpty())
                conStr = GlobalSwitch.DefaultDbConName;
            DbConnection dbConnection = DbProviderFactoryHelper.GetDbConnection(dbType);
            dbConnection.ConnectionString = DbProviderFactoryHelper.GetConStr(conStr);

            return dbConnection;
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
        private static void RefreshModel(string nameOrConStr, DatabaseType dbType)
        {
            var dbTypes = _dbCompiledModel.Keys.ToList();
            dbTypes.ForEach(aDbType =>
            {
                _dbCompiledModel[aDbType] = BuildDbCompiledModel(nameOrConStr, dbType);
            });
        }

        #endregion
    }
}
