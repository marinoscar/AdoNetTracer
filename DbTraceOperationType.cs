using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public enum DbTraceOperationType
    {
        None, OpenConnection, CloseConnection, ExecuteQuery, BeginTransaction, CommitTransaction, RollbackTransaction
    }
}
