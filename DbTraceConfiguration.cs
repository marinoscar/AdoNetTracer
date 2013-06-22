using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceConfiguration : ConfigurationSection 
    {

        [ConfigurationProperty("defaultTraceEventFormat", DefaultValue = "Text")]
        public DbTraceEventFormat DefaultTraceEventFormat
        {
            get { return (DbTraceEventFormat)Enum.Parse(typeof(DbTraceConfiguration), Convert.ToString(this["defaultTraceEventFormat"])); }
            set { this["defaultTraceEventFormat"] = Convert.ToString(value); }
        }
    }
}
