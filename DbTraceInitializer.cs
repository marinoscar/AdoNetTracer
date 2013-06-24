using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public static class DbTraceInitializer
    {
        public static void Initialize()
        {
            Database.DefaultConnectionFactory = new DbTraceConnectionFactory(Database.DefaultConnectionFactory);
        }
    }
}
