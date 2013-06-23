using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Data.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceProviderServices : DbProviderServices
    {
        public DbTraceProviderServices(DbProviderServices providerServices)
        {
            InternalServices = providerServices;
        }

        protected DbProviderServices InternalServices { get; private set; }

        #region Overriden Methods

        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            return new DbTraceCommandDefinition(InternalServices.CreateCommandDefinition(providerManifest, commandTree));
        }

        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            return InternalServices.GetProviderManifestToken(((DbTraceConnection)connection).InternalConnection);
        }

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return InternalServices.GetProviderManifest(manifestToken);
        }

        public override DbCommandDefinition CreateCommandDefinition(DbCommand prototype)
        {
            return  new DbTraceCommandDefinition(InternalServices.CreateCommandDefinition(prototype));
        }

        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            InternalServices.CreateDatabase(((DbTraceConnection)connection).InternalConnection, commandTimeout, storeItemCollection);
        }

        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            return InternalServices.CreateDatabaseScript(providerManifestToken, storeItemCollection);
        }

        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            return InternalServices.DatabaseExists(((DbTraceConnection)connection).InternalConnection, commandTimeout, storeItemCollection);
        }

        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            InternalServices.DeleteDatabase(((DbTraceConnection)connection).InternalConnection, commandTimeout, storeItemCollection);
        }

        #endregion
    }
}
