using System;
using System.Collections.Generic;
using UniDebug.Utils;
using UnityEngine;

namespace UniDebug
{
    public class ConsoleLogManager : IDisposable
    {
        public event Action<ConsoleLog> OnReceiveMessageEvent;
        public event Action OnClearLogEvent;

        private readonly RingBuffer<ConsoleLog> _allConsoleEntries;
        private readonly object _threadLock = new object();

        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public int InfoCount { get; private set; }

        public int AllLogCount => _allConsoleEntries.Count;

        public ConsoleLogManager(int logMax)
        {
            _allConsoleEntries = new RingBuffer<ConsoleLog>(logMax);
            Application.logMessageReceivedThreaded += UnityLogCallback;
            Application.lowMemory += ClearLog;
        }

        public void Dispose()
        {
            Application.logMessageReceivedThreaded -= UnityLogCallback;
        }

        private void UnityLogCallback(string condition, string stackTrace, LogType type)
        {
#if ENABLE_NAME_TRANSLATION
            condition = NameTranslationDecoder.Decode(condition);
            stackTrace = NameTranslationDecoder.Decode(stackTrace);
#endif
            var logData = new ConsoleLog(logtype: type, message: condition, stackTrace: stackTrace);
            lock (_threadLock)
            {
                _allConsoleEntries.Add(logData);
                AdjustCounter(type, 1);
                OnReceiveMessageEvent?.Invoke(logData);
            }
        }

        private void AdjustCounter(LogType type, int amount)
        {
            switch (type)
            {
                case LogType.Assert:
                case LogType.Error:
                case LogType.Exception:
                    ErrorCount += amount;
                    break;

                case LogType.Warning:
                    WarningCount += amount;
                    break;

                case LogType.Log:
                    InfoCount += amount;
                    break;
            }
        }

        public void ClearLog()
        {
            lock (_threadLock)
            {
                _allConsoleEntries.Clear();
                InfoCount = 0;
                WarningCount = 0;
                ErrorCount = 0;

                OnClearLogEvent?.Invoke();
            }
        }
    }
}
