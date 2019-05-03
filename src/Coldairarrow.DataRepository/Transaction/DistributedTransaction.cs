using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 数据库分布式事务,跨库事务
    /// </summary>
    public class DistributedTransaction : ITransaction
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repositories">其它数据仓储</param>
        public DistributedTransaction(params IRepository[] repositories)
        {
            if (repositories == null || repositories.Length == 0)
                throw new Exception("repositories不能为NULL且长度不为0");

            _repositorys = repositories.Distinct().ToList();
        }

        #endregion

        #region 内部成员

        private Dictionary<IRepository, bool?> _successDic { get; } = new Dictionary<IRepository, bool?>();
        private List<IRepository> _repositorys { get; }
        private void SetProperty(object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
        }
        private object GetProperty(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        }

        #endregion

        #region 外部接口

        public void BeginTransaction()
        {
            _repositorys.ForEach(aRepository =>
            {
                _successDic.Add(aRepository, null);
                (aRepository as DbRepository).BeginTransaction();
            });
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _repositorys.ForEach(aRepository =>
            {
                _successDic.Add(aRepository, null);
                (aRepository as DbRepository).BeginTransaction(isolationLevel);
            });
        }

        public (bool Success, Exception ex) EndTransaction()
        {
            bool isOK = true;
            Exception resEx = null;
            foreach (var aRepository in _repositorys)
            {
                try
                {
                    aRepository.GetDbContext().SaveChanges();
                    Action _sqlTransaction = GetProperty(aRepository, "_transactionHandler") as Action;
                    _sqlTransaction?.Invoke();
                    _successDic[aRepository] = true;
                }
                catch (Exception ex)
                {
                    resEx = ex;
                    _successDic[aRepository] = false;
                    isOK = false;
                    break;
                }
            }

            _repositorys.ForEach(aRepository =>
            {
                var transaction = GetProperty(aRepository, "Transaction") as DbContextTransaction;
                bool? success = _successDic[aRepository];
                if (isOK)
                    transaction.Commit();
                else
                {
                    if (success != null)
                        transaction.Rollback();
                }

                //释放初始化
                aRepository.GetType().GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(aRepository, null);
            });

            return (isOK, resEx);
        }

        #endregion
    }
}
