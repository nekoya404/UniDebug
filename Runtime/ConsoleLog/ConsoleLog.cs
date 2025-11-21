using UnityEngine;

namespace UniDebug
{
    public class ConsoleLog
    {
        public LogType LogType { get; }
        public string Message { get; }
        public string StackTrace { get; }

        public ConsoleLog(LogType logtype, string message, string stackTrace)
        {
            LogType = logtype;
            Message = message;
            StackTrace = stackTrace;
        }
    }
}
