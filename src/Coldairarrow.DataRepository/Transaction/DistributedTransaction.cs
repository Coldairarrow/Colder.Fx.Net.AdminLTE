using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Coldairarrow.DataRepository
{
    /// <summary>
    /// 数据库分布式事务,跨库事务
    /// </summary>
    public class DistributedTransaction : ITransaction, IDisposable
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

        private List<IRepository> _repositorys { get; }
        private Action _transactionHandler { get; set; }

        #endregion

        #region 外部接口

        /// <summary>
        /// 开始事物
        /// </summary>
        public void BeginTransaction()
        {
            _repositorys.ForEach(x => x.BeginTransaction());
        }

        /// <summary>
        /// 开始事物
        /// 注:自定义事物级别
        /// </summary>
        /// <param name="isolationLevel">事物级别</param>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _repositorys.ForEach(x => x.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// 添加事物操作
        /// </summary>
        /// <param name="action">事物操作</param>
        public void AddTransaction(Action action)
        {
            _transactionHandler += action;
        }

        /// <summary>
        /// 结束事物
        /// </summary>
        /// <returns></returns>
        public (bool Success, Exception ex) EndTransaction()
        {
            bool isOK = true;
            Exception resEx = null;
            try
            {
                _transactionHandler?.Invoke();
                _repositorys.ForEach(x => x.CommitDb());
                CommitTransaction();
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                isOK = false;
                resEx = ex;
            }
            finally
            {
                Dispose();
            }

            return (isOK, resEx);
        }

        /// <summary>
        /// 提交事物
        /// </summary>
        public void CommitTransaction()
        {
            _repositorys.ForEach(x => x.CommitTransaction());
        }

        /// <summary>
        /// 回滚事物
        /// </summary>
        public void RollbackTransaction()
        {
            _repositorys.ForEach(x => x.RollbackTransaction());
        }

        #endregion

        #region Dispose

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
            {
                _repositorys.ForEach(x => x.Dispose());
            }

            disposedValue = true;
        }

        ~DistributedTransaction()
        {
            Dispose(false);
        }

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
