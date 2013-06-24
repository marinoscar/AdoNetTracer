using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceCommand : DbCommand, IDbTracer
    {

        #region Constructors

        public DbTraceCommand(DbCommand command)
            : this(command, TryGetSessionIdFromCommand(command))
        {
        }

        public DbTraceCommand(DbCommand command, Guid sessionId)
        {
            InternalCommand = command;
            SessionId = sessionId;
        }

        #endregion

        #region Properties

        protected DbCommand InternalCommand { get; private set; }
        internal DbTraceConnection InternalConnection { get; set; }

        public Guid SessionId { get; private set; }

        public DbTraceEvents DbTraceEvents
        {
            get { return DbTraceEvents.Instance; }
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
            set
            {
                InternalConnection = value as DbTraceConnection;
                if (InternalConnection != null)
                {
                    InternalCommand.Connection = InternalConnection.InternalConnection;
                }
                else
                {
                    InternalConnection = new DbTraceConnection(value);
                    InternalCommand.Connection = InternalConnection.InternalConnection;
                }
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return InternalCommand.Parameters; }
        }


        protected override DbTransaction DbTransaction
        {
            get
            {
                return InternalCommand.Transaction == null ? null : new DbTraceTransaction(InternalCommand.Transaction);
            }
            set
            {
                var transaction = value as DbTraceTransaction;
                InternalCommand.Transaction = (transaction != null) ? transaction.InternalTransaction : value;
            }
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
            var dbEvent = DbTraceEvent.Start(SessionId, CommandText, DbTraceOperationType.ExecuteQuery);
            var result = InternalCommand.ExecuteReader(behavior);
            DbTraceEvents.TraceEvent(dbEvent.Stop());
            return result;
        }

        public override int ExecuteNonQuery()
        {
            var dbEvent = DbTraceEvent.Start(SessionId, CommandText, DbTraceOperationType.ExecuteQuery);
            var result = InternalCommand.ExecuteNonQuery();
            DbTraceEvents.TraceEvent(dbEvent.Stop());
            return result;
        }

        public override object ExecuteScalar()
        {
            var dbEvent = DbTraceEvent.Start(SessionId, CommandText, DbTraceOperationType.ExecuteQuery);
            var result = InternalCommand.ExecuteScalar();
            DbTraceEvents.TraceEvent(dbEvent.Stop());
            return result;
        }

        #endregion

        #region Method Implementation

        private static Guid TryGetSessionIdFromCommand(DbCommand command)
        {
            if (command != null && command.Connection != null &&
                command is DbTraceCommand && command.Connection is DbTraceConnection)
                return ((DbTraceConnection)command.Connection).SessionId;
            return Guid.NewGuid();
        }

        #endregion
    }
}
