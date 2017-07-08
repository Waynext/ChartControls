using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ULog
{
    public class LogHelper
    {
        private const string onlineLogName = "Online";
        //private const string generalLogName = "General";

        public static LogSource GeneralLogger;
        public static LogSource OnlineLogger;

        private static LogEventListener listener;

        public static void Init()
        {
            //GeneralLogger = new LogSource(generalLogName);
            OnlineLogger = new LogSource(onlineLogName);
            listener = new LogEventListener("App.log");
            listener.EnableEvents(OnlineLogger, EventLevel.Verbose);
        }

        public static void Release()
        {
            if(listener != null)
                listener.Dispose();
        }
    }
}
