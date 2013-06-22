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
        /// The session id for the event
        /// </summary>
        public Guid SessionId { get; internal set; }

        /// <summary>
        /// Start time of the event
        /// </summary>
        public DateTime StartTime { get; internal set; }
        /// <summary>
        /// Command of the operation
        /// </summary>
        public string Command { get; internal set; }
        /// <summary>
        /// Duration of the operation
        /// </summary>
        public TimeSpan Duration { get; internal set; }
        /// <summary>
        /// The type of operation being executed
        /// </summary>
        public DbTraceOperationType OperationType { get; private set; }

        #endregion

        internal DbTraceEvent()
        {
            StartTime = DateTime.Now;
            Duration = new TimeSpan(0);
            Command = string.Empty;
            OperationType = DbTraceOperationType.None;
            SessionId = Guid.Empty;
            _stopwatch = null;
        }

        private DbTraceEvent(Stopwatch start, string commandText, DbTraceOperationType operationType, Guid sessionId)
        {
            StartTime = DateTime.Now;
            Duration = new TimeSpan(0);
            Command = commandText;
            OperationType = operationType;
            SessionId = sessionId;
            _stopwatch = start;
        }

        public static DbTraceEvent Start(Guid sessionId, string commandText, DbTraceOperationType operationType)
        {
            return new DbTraceEvent(Stopwatch.StartNew(), commandText, operationType, sessionId);
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
