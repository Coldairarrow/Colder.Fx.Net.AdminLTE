using System;
using System.Data;

namespace Coldairarrow.DataRepository
{
    public interface ITransaction
    {
        void BeginTransaction();
        void BeginTransaction(IsolationLevel isolationLevel);
        (bool Success, Exception ex) EndTransaction();
    }
}
