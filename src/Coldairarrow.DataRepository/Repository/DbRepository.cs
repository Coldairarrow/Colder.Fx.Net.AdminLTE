using Coldairarrow.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 描述：数据库仓储基类类
    /// 作者：Coldairarrow
    /// </summary>
    public class DbRepository : IRepository
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="conString">构造参数，可以为数据库连接字符串或者DbContext</param>
        /// <param name="dbType">数据库类型</param>
        public DbRepository(string conString, DatabaseType dbType, string entityNamespace)
        {
            _conString = conString;
            _dbType = dbType;
            _entityNamespace = entityNamespace;
        }

        #endregion

        #region 私有成员

        protected IRepositoryDbContext Db
        {
            get
            {
                if (_isDisposed || _db == null)
                {
                    _db = DbFactory.GetDbContext(_conString, _dbType, _entityNamespace);
                    _db.Database.Log += HandleSqlLog;
                    _isDisposed = false;
                }

                return _db;
            }
            set
            {
                _db = value;
            }
        }
        private IRepositoryDbContext _db { get; set; }
        protected string _conString { get; set; }
        protected DatabaseType _dbType { get; set; }
        private string _entityNamespace { get; set; }
        protected bool _isDisposed { get; set; }
        protected DbContextTransaction Transaction { get; set; }
        protected static PropertyInfo GetKeyProperty(Type type)
        {
            return GetKeyPropertys(type).FirstOrDefault();
        }
        protected static List<PropertyInfo> GetKeyPropertys(Type type)
        {
            var properties = type
                .GetProperties()
                .Where(x => x.GetCustomAttributes(true).Select(o => o.GetType().FullName).Contains(typeof(KeyAttribute).FullName))
                .ToList();

            return properties;
        }
        protected string GetDbTableName(Type type)
        {
            string tableName = string.Empty;
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
                tableName = tableAttribute.Name;
            else
                tableName = type.Name;

            return tableName;
        }
        protected static ObjectQuery<T> GetObjectQueryFromDbQueryable<T>(IQueryable<T> query)
        {
            var internalQueryField = query.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals("_internalQuery")).FirstOrDefault();
            var internalQuery = internalQueryField.GetValue(query);
            var objectQueryField = internalQuery.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(f => f.Name.Equals("_objectQuery")).FirstOrDefault();
            return objectQueryField.GetValue(internalQuery) as ObjectQuery<T>;
        }

        #endregion

        #region 事物相关

        /// <summary>
        /// 是否开启事务,单库事务
        /// </summary>
        protected bool _openedTransaction { get; set; } = false;

        /// <summary>
        /// 需要执行的Sql事务
        /// </summary>
        protected Action _sqlTransaction { get; set; }

        /// <summary>
        /// 提交到数据库
        /// </summary>
        protected void Commit()
        {
            //若未开启事物则直接提交到数据库
            if (!_openedTransaction)
            {
                Db.SaveChanges();
                Db.Dispose();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// 释放数据,初始化状态
        /// </summary>
        protected void Dispose()
        {
            Transaction?.Dispose();
            Db?.Dispose();
            _isDisposed = true;
            _openedTransaction = false;
            _sqlTransaction = null;
        }

        /// <summary>
        /// 开始单库事物
        /// 注意:若要使用跨库事务,请使用DistributedTransaction
        /// </summary>
        public void BeginTransaction()
        {
            Transaction = Db.Database.BeginTransaction();
            _openedTransaction = true;
        }

        /// <summary>
        /// 结束事物提交
        /// </summary>
        public bool EndTransaction()
        {
            bool isOK = true;
            try
            {
                _sqlTransaction?.Invoke();
                Db.SaveChanges();
                Transaction.Commit();
            }
            catch
            {
                Transaction.Rollback();
                isOK = false;
            }
            finally
            {
                Dispose();
            }
            return isOK;
        }

        #endregion

        #region 数据库连接相关方法

        /// <summary>
        /// 获取DbContext
        /// </summary>
        /// <returns></returns>
        public DbContext GetDbContext()
        {
            return Db.GetDbContext();
        }

        /// <summary>
        /// 追踪SQL日志
        /// </summary>
        public Action<string> HandleSqlLog { get; set; }

        #endregion

        #region 增加数据

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="entity">实体</param>
        public void Insert(object entity)
        {
            Insert(new List<object> { entity });
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体</param>
        public void Insert<T>(T entity) where T : class, new()
        {
            Insert(new List<object> { entity });
        }

        /// <summary>
        /// 插入数据列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">实体列表</param>
        public void Insert<T>(List<T> entities) where T : class, new()
        {
            Insert(entities.CastToList<object>());
        }

        public void Insert(List<object> entities)
        {
            entities.ForEach(x => Db.Entry(x).State = EntityState.Added);
            Commit();
        }

        /// <summary>
        /// 使用Bulk批量插入数据（适合大数据量，速度非常快）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">数据</param>
        public virtual void BulkInsert<T>(List<T> entities) where T : class, new()
        {
            throw new NotImplementedException("不支持此操作!");
        }

        #endregion

        #region 删除数据

        public virtual void DeleteAll<T>() where T : class, new()
        {
            DeleteAll(typeof(T));
        }

        public virtual void DeleteAll(Type type)
        {
            string tableName = GetDbTableName(type);
            string sql = $"DELETE FROM {tableName}";
            ExecuteSql(sql);
        }

        public void Delete(object entity)
        {
            Delete(new List<object> { entity });
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体对象</param>
        public void Delete<T>(T entity) where T : class, new()
        {
            Delete(new List<object> { entity });
        }

        /// <summary>
        /// 删除多条数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">数据列表</param>
        public void Delete<T>(List<T> entities) where T : class, new()
        {
            Delete(entities.CastToList<object>());
        }

        public void Delete(List<object> entities)
        {
            foreach (var entity in entities)
            {
                Db.Entry(entity).State = EntityState.Deleted;
            }
            Commit();
        }

        /// <summary>
        /// 通过条件删除数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="condition">条件</param>
        public void Delete<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            var deleteList = GetIQueryable<T>().Where(condition).ToList();
            Delete(deleteList);
        }

        /// <summary>
        /// 通过条件删除数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="condition">条件</param>
        public virtual void Delete_Sql<T>(Expression<Func<T, bool>> condition) where T : class, new()
        {
            throw new NotImplementedException("不支持此操作!");
        }

        public void Delete<T>(string key) where T : class, new()
        {
            Delete<T>(new List<string> { key });
        }

        public void Delete<T>(List<string> keys) where T : class, new()
        {
            Delete(typeof(T), keys);
        }

        public void Delete(Type type, string key)
        {
            Delete(type, new List<string> { key });
        }

        public void Delete(Type type, List<string> keys)
        {
            var theProperty = GetKeyProperty(type);
            if (theProperty == null)
                throw new Exception("该实体没有主键标识！请使用[Key]标识主键！");

            List<object> deleteList = new List<object>();
            keys.ForEach(aKey =>
            {
                object newData = Activator.CreateInstance(type);
                var value = aKey.ChangeType(theProperty.PropertyType);
                theProperty.SetValue(newData, value);
                deleteList.Add(newData);
            });

            Delete(deleteList);
        }

        #endregion

        #region 更新数据

        public void Update(object entity)
        {
            Update(new List<object> { entity });
        }

        /// <summary>
        /// 默认更新一个实体，所有字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Update<T>(T entity) where T : class, new()
        {
            Update(new List<object> { entity });
        }

        /// <summary>
        /// 默认更新实体列表，所有字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void Update<T>(List<T> entities) where T : class, new()
        {
            Update(entities.CastToList<object>());
        }

        public void Update(List<object> entities)
        {
            entities.ForEach(aEntity =>
            {
                Db.Entry(aEntity).State = EntityState.Modified;
            });

            Commit();
        }

        /// <summary>
        /// 更新一条数据,某些属性
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="properties">需要更新的字段</param>
        public void UpdateAny<T>(T entity, List<string> properties) where T : class, new()
        {
            UpdateAny(new List<object> { entity }, properties);
        }

        public void UpdateAny(object entity, List<string> properties)
        {
            UpdateAny(new List<object> { entity }, properties);
        }

        /// <summary>
        /// 更新多条数据,某些属性
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">数据列表</param>
        /// <param name="properties">需要更新的字段</param>
        public void UpdateAny<T>(List<T> entities, List<string> properties) where T : class, new()
        {
            UpdateAny(entities.CastToList<object>(), properties);
        }

        public void UpdateAny(List<object> entities, List<string> properties)
        {
            entities.ForEach(aEntity =>
            {
                Db.Set(aEntity.GetType()).Attach(aEntity);
                properties.ForEach(aProperty =>
                {
                    Db.Entry(aEntity).Property(aProperty).IsModified = true;
                });
            });

            Commit();
        }

        /// <summary>
        /// 指定条件更新
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereExpre">筛选表达式</param>
        /// <param name="set">更改属性回调</param>
        public void UpdateWhere<T>(Expression<Func<T, bool>> whereExpre, Action<T> set) where T : class, new()
        {
            var list = GetIQueryable<T>().Where(whereExpre).ToList();
            list.ForEach(aData => set(aData));
            Update(list);
        }

        #endregion

        #region 查询数据

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="keyValue">主键</param>
        /// <returns></returns>
        public T GetEntity<T>(params object[] keyValue) where T : class, new()
        {
            return GetEntity(typeof(T), keyValue) as T;
        }

        public object GetEntity(Type type, params object[] keyValue)
        {
            var entity = Db.Set(type).Find(keyValue);
            Db.Entry(entity).State = EntityState.Detached;

            return entity;
        }

        /// <summary>
        /// 获取表的所有数据，当数据量很大时不要使用！
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public List<T> GetList<T>() where T : class, new()
        {
            return GetIQueryable<T>().ToList();
        }

        public List<object> GetList(Type type)
        {
            return GetIQueryable(type).CastToList<object>();
        }

        /// <summary>
        /// 获取实体对应的表，延迟加载，主要用于支持Linq查询操作
        /// 注意：无缓存
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns></returns>
        public IQueryable<T> GetIQueryable<T>() where T : class, new()
        {
            return GetIQueryable(typeof(T)) as IQueryable<T>;
        }

        /// <summary>
        /// 获取实体对应的表，延迟加载，主要用于支持Linq查询操作
        /// 注意：无缓存
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns></returns>
        public IQueryable GetIQueryable(Type type)
        {
            return Db.Set(type).AsNoTracking();
        }

        /// <summary>
        /// 通过Sql语句获取DataTable
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <returns></returns>
        public DataTable GetDataTableWithSql(string sql)
        {
            return GetDataTableWithSql(sql, null);
        }

        /// <summary>
        /// 通过Sql参数查询返回DataTable
        /// </summary>
        /// <param name="sql">Sql语句</param>
        /// <param name="parameters">查询参数</param>
        /// <returns></returns>
        public DataTable GetDataTableWithSql(string sql, List<DbParameter> parameters)
        {
            DbProviderFactory dbProviderFactory = DbProviderFactories.GetFactory(Db.Database.Connection);
            using (DbConnection conn = dbProviderFactory.CreateConnection())
            {
                conn.ConnectionString = _conString;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    cmd.CommandTimeout = 5 * 60;

                    if (parameters != null && parameters?.Count > 0)
                        cmd.Parameters.AddRange(parameters.ToArray());

                    DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();
                    adapter.SelectCommand = cmd;
                    DataSet table = new DataSet();
                    adapter.Fill(table);
                    cmd.Parameters.Clear();

                    return table.Tables[0];
                }
            }
        }

        /// <summary>
        /// 通过sql返回List
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sqlStr">sql语句</param>
        /// <returns></returns>
        public List<T> GetListBySql<T>(string sqlStr) where T : class, new()
        {
            return Db.Database.SqlQuery<T>(sqlStr).ToList();
        }

        /// <summary>
        /// 通过sql返回list
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="sqlStr">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public List<T> GetListBySql<T>(string sqlStr, List<DbParameter> parameters) where T : class, new()
        {
            return Db.Database.SqlQuery<T>(sqlStr, parameters.ToArray()).ToList();
        }


        #endregion

        #region 执行Sql语句

        /// <summary>
        /// 执行Sql语句
        /// </summary>
        /// <param name="sql">Sql语句</param>
        public void ExecuteSql(string sql)
        {
            if (!_openedTransaction)
            {
                Db.Database.ExecuteSqlCommand(sql);
                Dispose();
            }
            else
            {
                _sqlTransaction += new Action(() =>
                {
                    Db.Database.ExecuteSqlCommand(sql);
                });
            }
        }

        /// <summary>
        /// 通过参数执行Sql语句
        /// </summary>
        /// <param name="sql">Sql语句</param>
        public void ExecuteSql(string sql, List<DbParameter> parameters)
        {
            if (!_openedTransaction)
            {
                Db.Database.ExecuteSqlCommand(sql, parameters.ToArray());
                Dispose();
            }
            else
            {
                _sqlTransaction += new Action(() =>
                {
                    Db.Database.ExecuteSqlCommand(sql, parameters.ToArray());
                });
            }
        }

        #endregion
    }
}
