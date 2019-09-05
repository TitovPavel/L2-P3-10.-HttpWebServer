using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L2_P3_10.HttpWebServer.Contoller
{
    abstract class BaseController
    {

        protected string ServerDirectory { get; set; }

        protected BaseController(string serverDirectory)
        {
            ServerDirectory = serverDirectory;
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
