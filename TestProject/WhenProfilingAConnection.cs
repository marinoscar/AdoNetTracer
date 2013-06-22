using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdoNetTracer;
using NUnit.Framework;

namespace TestProject
{
    [TestFixture]
    public class WhenProfilingAConnection
    {
        [Test]
        public void ItShouldProvideInformationAboutAnOpeningAndClosingAConnection()
        {
            var traceConnection = GetConnection();
            traceConnection.Open();
            traceConnection.Close();
            Assert.IsTrue(traceConnection.DbTraceEvents.Events.Any(i => i.OperationType == DbTraceOperationType.OpenConnection));
            Assert.IsTrue(traceConnection.DbTraceEvents.Events.Any(i => i.OperationType == DbTraceOperationType.CloseConnection));
        }

        [Test]
        public void ItShouldProfileProperlyASelectStatement()
        {
            var traceConnection = GetConnection();
            var command = traceConnection.CreateCommand();
            command.CommandText = "select * from Items where Id = 5";
            traceConnection.Open();
            command.ExecuteScalar();
            traceConnection.Close();
            Assert.IsTrue(traceConnection.DbTraceEvents.Events.Any(i => i.Command == command.CommandText));
        }

        [Test]
        public void ItShouldPopulateDuration()
        {
            var traceConnection = GetConnection();
            traceConnection.Open();
            traceConnection.Close();
            var result = traceConnection.DbTraceEvents.Events.First();
            Assert.AreNotEqual(new TimeSpan(0), result.Duration);
        }

        [Test]
        public void ItShouldSerializeTheMessageAsJson()
        {
            SetConfigurationDefaultTraceFormat(DbTraceEventFormat.Json);
            var tracer = new DbTraceEvents();
            var sessionId = Guid.NewGuid();
            var eventItem = DbTraceEvent.Start(sessionId, "hello", DbTraceOperationType.ExecuteQuery);
            var result = tracer.SerializeEntry(eventItem.Stop());
            Assert.IsTrue(result.StartsWith("{"));
            Assert.IsTrue(result.EndsWith("}"));
            Assert.IsTrue(result.Contains(string.Format(@"""SessionId"":""{0}""", sessionId)));
        }

        private static void SetConfigurationDefaultTraceFormat(DbTraceEventFormat traceEventFormat)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var configSection = new DbTraceConfiguration { DefaultTraceEventFormat = traceEventFormat };
            if (
                configuration.Sections.Cast<ConfigurationSection>()
                             .Any(i => i.SectionInformation.Name == "dbTraceConfiguration"))
            {
                configSection = configuration.GetSection("dbTraceConfiguration") as DbTraceConfiguration;
                configSection.DefaultTraceEventFormat = traceEventFormat;
            }
            else
            {
                configuration.Sections.Add("dbTraceConfiguration", configSection);
            }
            configuration.Save(ConfigurationSaveMode.Full);
        }

        private DbTraceConnection GetConnection()
        {
            var connection = new SqlCeConnection(@"Data Source=.\Data\SampleData.sdf;Persist Security Info=False;");
            return new DbTraceConnection(connection);
        }
    }
}
