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
    public class DbTraceListener : TraceListener 
    {

        #region Constructors
        
        public DbTraceListener()
        {
            Events = new List<DbTraceEvent>();
            _configuration = GetConfigurationSection();
            if(_configuration.DefaultTraceEventFormat == DbTraceEventFormat.Json)
                _jsonSerializer = new JsonSerializer();
            if(_configuration.DefaultTraceEventFormat == DbTraceEventFormat.Xml)
                _xmlSerializer = new XmlSerializer(typeof(DbTraceEvent));
        } 

        #endregion

        #region Variable Declaration
        
        private readonly DbTraceConfiguration _configuration;
        private readonly XmlSerializer _xmlSerializer;
        private readonly JsonSerializer _jsonSerializer;
        private static DbTraceListener _instance;

        #endregion

        #region Property Implementation
        
        public List<DbTraceEvent> Events { get; set; } 
        public static DbTraceListener Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DbTraceListener();
                }
                return _instance;
            }
        }

        #endregion

        #region Overriden Methods

        public void TraceData(DbTraceEvent item)
        {
            Events.Add(item);
            WriteLine(SerializeEntry(item));
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (data is DbTraceEvent)
            {
                Events.Add(data as DbTraceEvent);
                WriteLine(SerializeEntry(data));
            }
            else
            {
                WriteLine(data.ToString());
            }
        }

        public override void Write(string message)
        {
            Debug.Write(message);
        }

        public override void WriteLine(string message)
        {
            Debug.WriteLine(message);
        } 

        #endregion

        #region Methods

        private string SerializeEntry(object item)
        {
            var stream = new StringWriter();
            switch (_configuration.DefaultTraceEventFormat)
            {
                case DbTraceEventFormat.Json:
                    _jsonSerializer.Serialize(stream, item);
                    break;
                case DbTraceEventFormat.Xml:
                    _xmlSerializer.Serialize(stream, item);
                    break;
                default:
                    stream.WriteLine(item);
                    break;
            }
            return stream.ToString();
        }

        private DbTraceConfiguration GetConfigurationSection()
        {
            var configSection = ConfigurationManager.GetSection("dbTraceConfiguration");
            if (configSection == null)
            {
                var newSection = new DbTraceConfiguration() { DefaultTraceEventFormat = DbTraceEventFormat.Text };
                return newSection;
            }
            return configSection as DbTraceConfiguration;
        } 

        #endregion
    }
}
