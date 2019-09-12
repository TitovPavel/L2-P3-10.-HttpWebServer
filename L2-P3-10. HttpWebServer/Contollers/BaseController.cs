using HttpWebServer.BL;
using HttpWebServer.Infrastructure;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace HttpWebServer.Contoller
{
    abstract class BaseController
    {

        public string ServerDirectory { get; set; }
        protected IParticipantsService ParticipantsService { get; set; }
        protected ILogger Logger { get; set; }

        protected BaseController(IParticipantsService participantsService, ILogger logger)
        {
            ParticipantsService = participantsService;
            Logger = logger;
            ServerDirectory = Program.serverDirectory;
        }
        public abstract void Handle(HttpListenerContext context);

        protected string GetResponse(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader file = new StreamReader(filePath))
                {
                    return file.ReadToEnd();
                }
            }

            return "";
        }

        protected void Send(HttpListenerContext context, string responseStr)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(responseStr);
            context.Response.ContentLength64 = buffer.Length;
            Stream output = context.Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        protected string GetPath(String path)
        {                  
            if (ServerDirectory == null)
            {
                return Path.GetFullPath(path);
            }
            else
            {
                return ServerDirectory + path;
            }
        }
    }
}
