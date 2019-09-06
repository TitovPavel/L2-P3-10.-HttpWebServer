namespace HttpWebServer.Infrastructure
{
    interface ILogger
    {
        void Error(string message);
        void Info(string message);
        void Warn(string message);
    }
}