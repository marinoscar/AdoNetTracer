using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
            :this(connection,providerFactory, Guid.NewGuid())
        {
        }

        public DbTraceConnection(DbConnection connection, DbProviderFactory providerFactory, Guid sessionId)
        {
            InternalConnection = connection;
            InternalProviderFactory = providerFactory;
            SessionId = sessionId;
        }

        #endregion

        #region Variable Declaration
        
        private bool _isFactoryWrapped; 

        #endregion

        #region Property Implementation

        protected DbProviderFactory InternalProviderFactory { get; private set; }
        internal DbConnection InternalConnection { get; set; }
        public Guid SessionId { get; private set; }

        public DbTraceEvents DbTraceEvents
        {
            get { return DbTraceEvents.Instance; }
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
            get
            {
                WrapProviders();
                return InternalProviderFactory;
            }
        }

        #endregion

        #region Overriden Methods

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var dbEvent = DbTraceEvent.Start(SessionId, "Begin transaction...", DbTraceOperationType.BeginTransaction);
            var result = InternalConnection.BeginTransaction(isolationLevel);
            DbTraceEvents.TraceEvent(dbEvent.Stop());
            return result;
        }

        public override void Close()
        {
            var dbEvent = DbTraceEvent.Start(SessionId, "Closing connection...", DbTraceOperationType.CloseConnection);
            InternalConnection.Close();
            DbTraceEvents.TraceEvent(dbEvent.Stop());
        }

        public override void ChangeDatabase(string databaseName)
        {
            InternalConnection.ChangeDatabase(databaseName);
        }

        public override void Open()
        {
            var dbEvent = DbTraceEvent.Start(SessionId, "Opening connection...", DbTraceOperationType.OpenConnection);
            InternalConnection.Open();
            DbTraceEvents.TraceEvent(dbEvent.Stop());
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

        /// <summary>
        /// Wraps the existing providers with the AdoTraceProviders
        /// </summary>
        private void WrapProviders()
        {
            if (_isFactoryWrapped) return;
            try
            {
                DbProviderFactories.GetFactory("Anything");
            }
            catch (ArgumentException)
            {
            }
            var registeredProviders = GetRegisteredProviders();
            foreach (var row in registeredProviders.Rows.Cast<DataRow>().ToList())
            {
                DbProviderFactory factory = null;
                try
                {
                    factory = DbProviderFactories.GetFactory(row);
                }
                catch (Exception)
                {
                    continue;
                }

                if (factory is DbTraceProviderFactory)
                {
                    continue;
                }

                var proxyType = typeof(DbTraceProviderFactory<>).MakeGenericType(factory.GetType());

                var newRow = registeredProviders.NewRow();
                newRow["Name"] = row["Name"];
                newRow["Description"] = row["Description"];
                newRow["InvariantName"] = row["InvariantName"];
                newRow["AssemblyQualifiedName"] = proxyType.AssemblyQualifiedName;

                registeredProviders.Rows.Remove(row);
                registeredProviders.Rows.Add(newRow);
            }
            _isFactoryWrapped = true;
        }

        public DataTable GetRegisteredProviders()
        {
            var dbProviderFactories = typeof(DbProviderFactories);
            var providerField = dbProviderFactories.GetField("_configTable", BindingFlags.NonPublic | BindingFlags.Static) ?? dbProviderFactories.GetField("_providerTable", BindingFlags.NonPublic | BindingFlags.Static);
            var registrations = providerField.GetValue(null);
            return registrations is DataSet ? ((DataSet)registrations).Tables["DbProviderFactories"] : (DataTable)registrations;
        }

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
