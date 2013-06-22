using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace AdoNetTracer
{
    public class DbTraceConnection : DbConnection, IDbTracer
    {
        #region Constructors

        public DbTraceConnection(DbConnection connection)
            : this(connection, GetProviderFactory(connection))
        {

        }

        public DbTraceConnection(DbConnection connection, DbProviderFactory providerFactory)
        {
            InternalConnection = connection;
            InternalProviderFactory = providerFactory;
        }

        #endregion

        #region Property Implementation

        protected DbProviderFactory InternalProviderFactory { get; private set; }
        protected DbConnection InternalConnection { get; set; }
        public DbTraceListener Tracer
        {
            get { return DbTraceListener.Instance; }
        }

        #endregion

        #region Overriden Properties

        public override System.ComponentModel.ISite Site
        {
            get { return InternalConnection.Site; }
            set { InternalConnection.Site = value; }
        }

        public override string ConnectionString
        {
            get { return InternalConnection.ConnectionString; }
            set { InternalConnection.ConnectionString = value; }
        }

        public override string Database
        {
            get { return InternalConnection.Database; }
        }

        public override ConnectionState State
        {
            get { return InternalConnection.State; }
        }

        public override string DataSource
        {
            get { return InternalConnection.DataSource; }
        }

        public override string ServerVersion
        {
            get { return InternalConnection.ServerVersion; }
        }

        public override int ConnectionTimeout
        {
            get { return InternalConnection.ConnectionTimeout; }
        }

        protected override DbProviderFactory DbProviderFactory
        {
            get { return InternalProviderFactory; }
        }

        #endregion

        #region Overriden Methods

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var dbEvent = DbTraceEvent.Start("Begin transaction...", DbTraceOperationType.BeginTransaction);
            var result = InternalConnection.BeginTransaction(isolationLevel);
            Tracer.TraceData(dbEvent.Stop());
            return result;
        }

        public override void Close()
        {
            var dbEvent = DbTraceEvent.Start("Closing connection...", DbTraceOperationType.CloseConnection);
            InternalConnection.Close();
            Tracer.TraceData(dbEvent.Stop());
        }

        public override void ChangeDatabase(string databaseName)
        {
            InternalConnection.ChangeDatabase(databaseName);
        }

        public override void Open()
        {
            var dbEvent = DbTraceEvent.Start("Opening connection...", DbTraceOperationType.OpenConnection);
            InternalConnection.Open();
            Tracer.TraceData(dbEvent.Stop());
        }

        protected override DbCommand CreateDbCommand()
        {
            return new DbTraceCommand(InternalConnection.CreateCommand());
        }

        public override DataTable GetSchema()
        {
            return InternalConnection.GetSchema();
        }

        public override DataTable GetSchema(string collectionName)
        {
            return InternalConnection.GetSchema(collectionName);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictionValues)
        {
            return InternalConnection.GetSchema(collectionName, restrictionValues);
        }

        protected override object GetService(Type service)
        {
            return ((IServiceProvider)InternalConnection).GetService(service);
        }

        protected override void Dispose(bool disposing)
        {
            InternalConnection.Dispose();
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            InternalConnection.EnlistTransaction(transaction);
        }

        #endregion

        #region Methods

        private static DbProviderFactory GetProviderFactory(DbConnection connection)
        {
            var factory = DbProviderFactories.GetFactory(connection);
            var traceFactory = typeof(DbTraceProviderFactory<>).MakeGenericType(factory.GetType());
            Activator.CreateInstance(traceFactory);
            return Activator.CreateInstance(traceFactory) as DbProviderFactory;
        }

        #endregion
    }
}
