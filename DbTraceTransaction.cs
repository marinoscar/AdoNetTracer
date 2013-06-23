using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceTransaction : DbTransaction, IDbTracer
    {

        #region Constructors

        public DbTraceTransaction(DbTransaction transaction) : this(transaction, TryGetSessionIdFromTransaction(transaction))
        {
        }


        public DbTraceTransaction(DbTransaction transaction, Guid sessionId)
        {
            InternalTransaction = transaction;
            SessionId = sessionId;
        } 



        #endregion

        #region Propertpy Implementation
        
        internal DbTransaction InternalTransaction { get; private set; }
        public DbTraceEvents DbTraceEvents
        {
            get { return DbTraceEvents.Instance; }
        }

        public Guid SessionId { get; private set; }

        #endregion

        #region Overriden Methods
        
        public override void Commit()
        {
            var dbEvent = DbTraceEvent.Start(SessionId, "Commiting transaction", DbTraceOperationType.CommitTransaction);
            InternalTransaction.Commit();
            DbTraceEvents.TraceEvent(dbEvent.Stop());
        }

        public override void Rollback()
        {
            var dbEvent = DbTraceEvent.Start(SessionId, "Rolling back transaction", DbTraceOperationType.RollbackTransaction);
            InternalTransaction.Rollback();
            DbTraceEvents.TraceEvent(dbEvent.Stop());
        } 

        #endregion

        #region Overriden Properties
        
        protected override DbConnection DbConnection
        {
            get { return InternalTransaction.Connection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get { return InternalTransaction.IsolationLevel; }
        } 

        #endregion

        #region Method Implementation

        private static Guid TryGetSessionIdFromTransaction(DbTransaction transaction)
        {
            if (transaction != null && transaction.Connection != null &&
                transaction is DbTraceTransaction && transaction.Connection is DbTraceConnection)
                return ((DbTraceConnection)transaction.Connection).SessionId;
            return Guid.NewGuid();
        }

        #endregion
    }
}
