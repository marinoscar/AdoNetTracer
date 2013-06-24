using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceConfigurationSection : ConfigurationSection 
    {

        [ConfigurationProperty("defaultTraceEventFormat", DefaultValue = "Text")]
        public DbTraceEventFormat DefaultTraceEventFormat
        {
            get { return (DbTraceEventFormat)Enum.Parse(typeof(DbTraceEventFormat), Convert.ToString(this["defaultTraceEventFormat"])); }
            set { this["defaultTraceEventFormat"] = Convert.ToString(value); }
        }
    }
}
