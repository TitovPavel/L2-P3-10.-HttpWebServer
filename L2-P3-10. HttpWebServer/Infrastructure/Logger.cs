using System;
using System.IO;

namespace HttpWebServer.Infrastructure
{
    class Logger : ILogger
    {
        readonly string path = "log.txt";
        public void Info(string message)
        {
            WriteLog($"{DateTime.Now} - Info: {message}");
        }

        public void Warn(string message)
        {
            WriteLog($"{DateTime.Now} - Warn: {message}");
        }

        public void Error(string message)
        {
            WriteLog($"{DateTime.Now} - Error: {message}");
        }

        void WriteLog(string message)
        {
            using (StreamWriter fs = new StreamWriter(path, true))
            {
                fs.WriteLine(message);
            }
        }
    }
}
