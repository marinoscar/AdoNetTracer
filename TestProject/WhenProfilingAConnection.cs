using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
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
        public void ItShouldProvideInformationAboutAnOpenConnection()
        {
            var connection = new SqlCeConnection(@"Data Source=.\Data\SampleData.sdf;Persist Security Info=False;");
            var traceConnection = new DbTraceConnection(connection);
            traceConnection.Open();
        }
    }
}
