using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MpgLogger
{
    public class MpgFileLogger
    {
        public static MpgFileLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MpgFileLogger();
                    return _instance;
                }
                return _instance;
            }
        }

        public string LogFilePath { get; set; }

        private static MpgFileLogger? _instance;

        private object _debugFileLock = new();
        private object _errorFileLock = new();

        private ConcurrentQueue<string> _debugMessagesToLog = new();
        private ConcurrentQueue<string> _errorMessagesToLog = new();

        public MpgFileLogger()
        {
            LogFilePath = "logs/";
            Directory.CreateDirectory(LogFilePath);

            new Thread(ConsumeMessages).Start();
        }

        public void Log(string message, LogType logType = LogType.Debug)
        {
            switch (logType)
            {
                case LogType.Debug:
                    _debugMessagesToLog.Enqueue(message);
                    break;
                case LogType.Error:
                    _errorMessagesToLog.Enqueue(message);
                    _debugMessagesToLog.Enqueue(message);
                    break;
            }
        }

        private void ConsumeMessages()
        {
            while (true)
            {
                ProcessQueue(ref _debugMessagesToLog, "debug", ref _debugFileLock);
                ProcessQueue(ref _errorMessagesToLog, "error", ref _errorFileLock);
            }
        }

        private void ProcessQueue(ref ConcurrentQueue<string> queue, string logFile, ref object fileLock)
        {
            string currentDate = DateTime.Now.Date.ToString("dd-MM-yyyy");
            if (!queue.TryDequeue(out string message))
            {
                return;
            }

            lock (fileLock)
            {
                using (var file = new StreamWriter($"{LogFilePath}{currentDate}_{logFile}.txt", true))
                {
                    string currentTime = DateTime.Now.TimeOfDay.ToString();
                    string messageToWrite = $"{currentTime} : {message}";
                    file.WriteLine(messageToWrite);
                    file.Close();
                }
            }
        }

        public enum LogType
        {
            Debug, Error
        }
    }
}
