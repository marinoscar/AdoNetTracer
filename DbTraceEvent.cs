using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetTracer
{
    public class DbTraceEvent
    {
        private readonly Stopwatch _stopwatch;

        #region Properties

        /// <summary>
        /// Start time of the event
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// Command of the operation
        /// </summary>
        public string Command { get; private set; }
        /// <summary>
        /// Duration of the operation
        /// </summary>
        public TimeSpan Duration { get; private set; }

        public DbTraceOperationType OperationType { get; private set; }

        #endregion

        private DbTraceEvent(Stopwatch start, string commandText, DbTraceOperationType operationType)
        {
            StartTime = DateTime.Now;
            Duration = new TimeSpan(0);
            Command = commandText;
            OperationType = operationType;
            _stopwatch = start;
        }

        public static DbTraceEvent Start(string commandText, DbTraceOperationType operationType)
        {
            return new DbTraceEvent(Stopwatch.StartNew(), commandText, operationType);
        }

        public DbTraceEvent Stop()
        {
            _stopwatch.Stop();
            Duration = _stopwatch.Elapsed;
            return this;
        }

        public override string ToString()
        {
            return string.Format("Started On: [{0}] Duration: [{1}]\nMessage: {2}", StartTime, Duration, Command);
        }
    }
}
