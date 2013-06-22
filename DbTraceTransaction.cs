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

        public DbTraceTransaction(DbTransaction transaction)
        {
            InternalTransaction = transaction;
        } 

        #endregion

        #region Propertpy Implementation
        
        protected DbTransaction InternalTransaction { get; private set; }
        public DbTraceListener Tracer
        {
            get { return DbTraceListener.Instance; }
        }

        #endregion

        #region Overriden Methods
        
        public override void Commit()
        {
            var dbEvent = DbTraceEvent.Start("Commiting transaction", DbTraceOperationType.CommitTransaction);
            InternalTransaction.Commit();
            Tracer.TraceData(dbEvent.Stop());
        }

        public override void Rollback()
        {
            var dbEvent = DbTraceEvent.Start("Rolling back transaction", DbTraceOperationType.RollbackTransaction);
            InternalTransaction.Rollback();
            Tracer.TraceData(dbEvent.Stop());
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
    }
}
