using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceCommandDefinition : DbCommandDefinition
    {
        internal DbCommandDefinition InternalCommandDefinition { get; private set; }

        public DbTraceCommandDefinition(DbCommandDefinition commandDefinition)
        {
            InternalCommandDefinition = commandDefinition;
        }

        public override DbCommand CreateCommand()
        {
            return new DbTraceCommand(InternalCommandDefinition.CreateCommand());
        }
    }
}
