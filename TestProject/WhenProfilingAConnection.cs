using System;
using System.Collections.Generic;
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
            Assert.IsTrue(traceConnection.Tracer.Events.Any(i => i.OperationType == DbTraceOperationType.OpenConnection));
            Assert.IsTrue(traceConnection.Tracer.Events.Any(i => i.OperationType == DbTraceOperationType.CloseConnection));
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
            Assert.IsTrue(traceConnection.Tracer.Events.Any(i => i.Command == command.CommandText));
        }

        private DbTraceConnection GetConnection()
        {
            var connection = new SqlCeConnection(@"Data Source=.\Data\SampleData.sdf;Persist Security Info=False;");
            return new DbTraceConnection(connection);
        }
    }
}
