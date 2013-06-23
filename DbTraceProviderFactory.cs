using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public abstract class DbTraceProviderFactory : DbProviderFactory 
    {
    }

    public class DbTraceProviderFactory<TProvider> : DbTraceProviderFactory, IServiceProvider where TProvider : DbProviderFactory
    {
        #region Constructors
        
        public DbTraceProviderFactory()
        {
            var instance = typeof(TProvider).GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instance == null)
            {
                throw new NotSupportedException("Provider does not has an instance");
            }

            InternalFactory = (TProvider)instance.GetValue(null);
        } 

        #endregion

        #region Variable Declaration
        
        public static readonly DbTraceProviderFactory<TProvider> Instance = new DbTraceProviderFactory<TProvider>(); 

        #endregion

        #region Property Implementation
        
        protected TProvider InternalFactory { get; private set; } 

        #endregion

        #region Overriden Methods
        
        public override bool CanCreateDataSourceEnumerator
        {
            get { return InternalFactory.CanCreateDataSourceEnumerator; }
        }

        public override DbConnection CreateConnection()
        {
            return new DbTraceConnection(InternalFactory.CreateConnection());
        }

        public override DbCommand CreateCommand()
        {
            return new DbTraceCommand(InternalFactory.CreateCommand());
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return InternalFactory.CreateCommandBuilder();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return InternalFactory.CreateConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return InternalFactory.CreateDataAdapter();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return InternalFactory.CreateDataSourceEnumerator();
        }

        public override DbParameter CreateParameter()
        {
            return InternalFactory.CreateParameter();
        }

        public override System.Security.CodeAccessPermission CreatePermission(PermissionState state)
        {
            return InternalFactory.CreatePermission(state);
        }

        #endregion

        public object GetService(Type serviceType)
        {
            if (serviceType == GetType())
                return InternalFactory;
            var service = ((IServiceProvider)InternalFactory).GetService(serviceType);
            if (serviceType == typeof (DbProviderServices))
                return Activator.CreateInstance(typeof (DbTraceProviderServices), service);
            return service;
        }
    }

}
