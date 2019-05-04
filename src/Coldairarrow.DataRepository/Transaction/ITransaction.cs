using System;
using System.Data;

namespace Coldairarrow.DataRepository
{
    public interface ITransaction
    {
        /// <summary>
        /// 开始事物
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// 开始事物
        /// 注:自定义事物级别
        /// </summary>
        /// <param name="isolationLevel">事物级别</param>
        void BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// 添加事物操作
        /// </summary>
        /// <param name="action">事物操作</param>
        void AddTransaction(Action action);

        /// <summary>
        /// 提交事物
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// 回滚事物
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// 结束事物
        /// </summary>
        /// <returns></returns>
        (bool Success, Exception ex) EndTransaction();
    }
}
