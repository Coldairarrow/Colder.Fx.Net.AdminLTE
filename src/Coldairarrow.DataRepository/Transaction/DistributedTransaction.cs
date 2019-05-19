using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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
            if (repositories == null)
                throw new Exception("repositories不能为NULL");

            if (repositories.Length > 0)
                _repositorys = repositories.Distinct().ToList();
        }

        #endregion

        #region 内部成员

        private List<IRepository> _repositorys { get; } = new List<IRepository>();
        private ITransaction _BeginTransaction(IsolationLevel? isolationLevel = null)
        {
            List<Task> tasks = new List<Task>();

            _repositorys.ForEach(x =>
            {
                tasks.Add(Task.Run(() =>
                {
                    Begin(x);
                }));
            });

            Task.WaitAll(tasks.ToArray());

            return this;

            void Begin(ITransaction db)
            {
                if (isolationLevel == null)
                    db.BeginTransaction();
                else
                    db.BeginTransaction(isolationLevel.Value);
            }
        }

        #endregion

        #region 外部接口

        public void AddRepository(params IRepository[] repositories)
        {
            _repositorys.AddRange(repositories.Distinct());
        }

        /// <summary>
        /// 开始事物
        /// </summary>
        public ITransaction BeginTransaction()
        {
            return _BeginTransaction();
        }

        /// <summary>
        /// 开始事物
        /// 注:自定义事物级别
        /// </summary>
        /// <param name="isolationLevel">事物级别</param>
        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// 结束事物
        /// </summary>
        /// <returns></returns>
        public (bool Success, Exception ex) EndTransaction()
        {
            if (_repositorys.Count == 0)
                throw new Exception("IRepository数量不能为0");

            bool isOK = true;
            Exception resEx = null;
            try
            {
                CommitDb();
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

        public void CommitDb()
        {
            List<Task> tasks = new List<Task>();

            _repositorys.ForEach(x =>
            {
                tasks.Add(Task.Run(() =>
                {
                    x.CommitDb();
                }));
            });

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// 提交事物
        /// </summary>
        public void CommitTransaction()
        {
            List<Task> tasks = new List<Task>();

            _repositorys.ForEach(x =>
            {
                tasks.Add(Task.Run(() =>
                {
                    x.CommitTransaction();
                }));
            });

            Task.WaitAll(tasks.ToArray());
            //_repositorys.ForEach(x => x.CommitTransaction());
        }

        /// <summary>
        /// 回滚事物
        /// </summary>
        public void RollbackTransaction()
        {
            List<Task> tasks = new List<Task>();

            _repositorys.ForEach(x =>
            {
                tasks.Add(Task.Run(() =>
                {
                    x.RollbackTransaction();
                }));
            });

            Task.WaitAll(tasks.ToArray());

            //_repositorys.ForEach(x => x.RollbackTransaction());
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
