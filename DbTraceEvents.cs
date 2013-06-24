using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace AdoNetTracer
{
    public class DbTraceEvents 
    {

        #region Constructors
        
        public DbTraceEvents()
        {
            Events = new List<DbTraceEvent>();
            _configuration = GetConfigurationSection();
            if(_configuration.DefaultTraceEventFormat == DbTraceEventFormat.Json)
                _jsonSerializer = new JsonSerializer();
        } 

        #endregion

        #region Variable Declaration
        
        private readonly DbTraceConfigurationSection _configuration;
        private readonly JsonSerializer _jsonSerializer;
        private static DbTraceEvents _instance;

        #endregion

        #region Property Implementation
        
        public List<DbTraceEvent> Events { get; set; } 
        public static DbTraceEvents Instance
        {
            get { return _instance ?? (_instance = new DbTraceEvents()); }
        }

        #endregion

        #region Methods

        public void TraceEvent(DbTraceEvent item)
        {
            Events.Add(item);
            Trace.WriteLine(SerializeEntry(item));
        }

        public string SerializeEntry(object item)
        {
            var stream = new StringWriter();
            switch (_configuration.DefaultTraceEventFormat)
            {
                case DbTraceEventFormat.Json:
                    _jsonSerializer.Serialize(stream, item);
                    break;
                default:
                    stream.WriteLine(item);
                    break;
            }
            return stream.ToString();
        }

        private DbTraceConfigurationSection GetConfigurationSection()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var configSection = configuration.GetSection("dbTraceConfiguration") as DbTraceConfigurationSection;
            if (configSection == null)
            {
                var newSection = new DbTraceConfigurationSection() { DefaultTraceEventFormat = DbTraceEventFormat.Text };
                return newSection;
            }
            return configSection;
        } 

        #endregion
    }
}
