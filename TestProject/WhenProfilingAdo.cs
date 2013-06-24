using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
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
    public class WhenProfilingAdo
    {

        #region Test Setup
        
        [TestFixtureSetUp]
        public void SetupTest()
        {
            Database.SetInitializer<SampleDataContext>(null);
            InitializeData();
            DbTraceInitializer.Initialize();
        } 

        #endregion

        #region Test Cases

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

        [Test]
        public void ItShouldProperlyProfileEntityFrameworkQueries()
        {
            var connection = GetConnection();
            var context = new SampleDataContext(connection);
            connection.DbTraceEvents.Events.Clear();
            var query = (from c in context.Items where c.Id > 2 select c);
            var result = query.ToString(); //Force the query to be executed
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(connection.DbTraceEvents.Events.Count > 0);
        }

        #endregion

        #region Supporting Methods
        
        private static void SetConfigurationDefaultTraceFormat(DbTraceEventFormat traceEventFormat)
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var configSection = new DbTraceConfigurationSection { DefaultTraceEventFormat = traceEventFormat };
            if (
                configuration.Sections.Cast<ConfigurationSection>()
                             .Any(i => i.SectionInformation.Name == "dbTraceConfiguration"))
            {
                configSection = configuration.GetSection("dbTraceConfiguration") as DbTraceConfigurationSection;
                configSection.DefaultTraceEventFormat = traceEventFormat;
            }
            else
            {
                configuration.Sections.Add("dbTraceConfiguration", configSection);
            }
            configuration.Save(ConfigurationSaveMode.Full);
        }

        private static DbTraceConnection GetConnection()
        {
            var connection = new SqlCeConnection(@"Data Source=.\Data\SampleData.sdf;Persist Security Info=False;");
            return new DbTraceConnection(connection);
        } 

        private void InitializeData()
        {
            var connection = GetConnection();
            ExecuteSql(connection, "DELETE FROM Items");
            ExecuteSql(connection, "INSERT INTO Items VALUES (1, 'ABC', 'ABC', '1/1/2013', 'User')");
            ExecuteSql(connection, "INSERT INTO Items VALUES (2, 'DEF', 'DEF', '1/1/2013', 'User')");
            ExecuteSql(connection, "INSERT INTO Items VALUES (3, 'GHI', 'GHI', '1/1/2013', 'User')");
            ExecuteSql(connection, "INSERT INTO Items VALUES (4, 'JKL', 'JKL', '1/1/2013', 'User')");
        }

        private static void ExecuteSql(DbConnection connection, string sql)
        {
            var cmd = connection.CreateCommand();
            connection.Open();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            connection.Close();
            cmd.Dispose();
        }

        #endregion
    }
}
