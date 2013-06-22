using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceCommand : DbCommand
    {

        #region Constructors

        public DbTraceCommand(DbCommand command)
        {
            InternalCommand = command;
        }

        #endregion

        #region Properties

        protected DbCommand InternalCommand { get; private set; }
        protected DbTraceListener Trace
        {
            get { return DbTraceListener.Instance; }
        }

        #endregion

        #region Overriden Properties

        public override string CommandText
        {
            get { return InternalCommand.CommandText; }
            set { InternalCommand.CommandText = value; }
        }
        public override int CommandTimeout
        {
            get { return InternalCommand.CommandTimeout; }
            set { InternalCommand.CommandTimeout = value; }
        }
        public override CommandType CommandType
        {
            get { return InternalCommand.CommandType; }
            set { InternalCommand.CommandType = value; }
        }
        public override UpdateRowSource UpdatedRowSource
        {
            get { return InternalCommand.UpdatedRowSource; }
            set { InternalCommand.UpdatedRowSource = value; }
        }
        protected override DbConnection DbConnection
        {
            get { return InternalCommand.Connection; }
            set { InternalCommand.Connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return InternalCommand.Parameters; }
        }


        protected override DbTransaction DbTransaction
        {
            get { return InternalCommand.Transaction; }
            set { InternalCommand.Transaction = value; }
        }
        public override bool DesignTimeVisible
        {
            get { return InternalCommand.DesignTimeVisible; }
            set { InternalCommand.DesignTimeVisible = value; }
        }


        #endregion

        #region Overridden Methods

        public override void Prepare()
        {
            InternalCommand.Prepare();
        }

        public override void Cancel()
        {
            InternalCommand.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return InternalCommand.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var dbEvent = DbTraceEvent.Start(CommandText, DbTraceOperationType.ExecuteQuery);
            var result = InternalCommand.ExecuteReader(behavior);
            Trace.TraceData(dbEvent.Stop());
            return result;
        }

        public override int ExecuteNonQuery()
        {
            var dbEvent = DbTraceEvent.Start(CommandText, DbTraceOperationType.ExecuteQuery);
            var result = InternalCommand.ExecuteNonQuery();
            Trace.TraceData(dbEvent.Stop());
            return result;
        }

        public override object ExecuteScalar()
        {
            var dbEvent = DbTraceEvent.Start(CommandText, DbTraceOperationType.ExecuteQuery);
            var result = InternalCommand.ExecuteScalar();
            Trace.TraceData(dbEvent.Stop());
            return result;
        }

        #endregion
    }
}
