using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULog
{
    public class LogSource : EventSource
    {
        private string messageFormat = "\t{0}";
        public LogSource(string name)
        {
            messageFormat = name + messageFormat;
        }

        [Event(1, Level = EventLevel.Verbose)]
        public void Debug(string message)
        {
            this.WriteEvent(1, string.Format(messageFormat, message));
        }

        [Event(2, Level = EventLevel.Informational)]
        public void Info(string message)
        {
            this.WriteEvent(2, string.Format(messageFormat, message));
        }

        [Event(3, Level = EventLevel.Warning)]
        public void Warn(string message)
        {
            this.WriteEvent(3, string.Format(messageFormat, message));
        }

        [Event(4, Level = EventLevel.Error)]
        public void Error(string message)
        {
            this.WriteEvent(4, string.Format(messageFormat, message));
        }

        [Event(5, Level = EventLevel.Critical)]
        public void Critical(string message)
        {
            this.WriteEvent(5, string.Format(messageFormat, message));
        }
    }

    public class OnlineLogSource : LogSource
    {
        public OnlineLogSource() : base("Online") { }
    }
}
