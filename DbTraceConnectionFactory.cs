using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceConnectionFactory : IDbConnectionFactory
    {

        public DbTraceConnectionFactory(IDbConnectionFactory connectionFactory)
        {
            InternalConnectionFactory = connectionFactory;
        }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            var connection = InternalConnectionFactory.CreateConnection(nameOrConnectionString);
            return connection is DbTraceConnection ? connection : new DbTraceConnection(connection);
        }

        protected IDbConnectionFactory InternalConnectionFactory { get; private set; }
    }
}
