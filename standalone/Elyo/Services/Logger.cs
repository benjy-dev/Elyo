using System;
using System.IO;

namespace Elyo.Services
{
    public static class Logger
    {
        private static string _logFile;

        public static void Init(string path)
        {
            _logFile = path;
            File.WriteAllText(_logFile, $"[LOG DÉMARRÉ] {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        }

        public static void Log(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(_logFile, logEntry);
        }
    }
}
