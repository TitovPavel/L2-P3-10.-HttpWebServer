using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace L2_P3_10.HttpWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server ws = new Server("http://*:8881/");
            ws.Run();

            Console.WriteLine("Press a key to quit.");
            Console.ReadKey();

            ws.Stop();

        }
    }

    class Server
    {
        private HttpListener listener;

        private string serverDirectory;

        const string mainPage = "index.html";


        public Server(string URI)
        {
            ReadSettings();
            listener = new HttpListener();
            listener.Prefixes.Add(URI);
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void ReadSettings()
        {
            using (StreamReader sr = new StreamReader("Settings.txt"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if(line.Contains("serverDirectory="))
                    {
                        serverDirectory = line.Replace("serverDirectory=", "");
                    }
                }
            }
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("Webserver running...");

                while (listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem(WriteToClient, listener.GetContext());
                }
            });
        }

        public void Stop()
        {
            listener.Stop();
            listener.Close();
        }



        private void WriteToClient(object obj)
        {

            var context = obj as HttpListenerContext;
            try
            {
                if (context == null)
                {
                    return;
                }

                HttpListenerResponse response = context.Response;
                HttpListenerRequest request = context.Request;

                if (context.Request.HttpMethod.Equals("GET"))
                {
                    string filePath = GetPath(request.Url.AbsolutePath);
                    if (!File.Exists(filePath))
                    {
                        response.StatusCode = 404;
                        return;
                    }

                    string ifModifiedSince = request.Headers.Get("if-Modified-Since");
                    DateTime dateTimeModified;
                    
                    if (ifModifiedSince == null || ( DateTime.TryParse(ifModifiedSince, out dateTimeModified) && dateTimeModified.AddMinutes(2) < DateTime.Now ) )
                    {
                        SetLastModified(response);
                    }
                    else
                    {
                        response.StatusCode = 304;
                        return;
                    }

                    using (StreamReader file = new StreamReader(filePath))
                    {
                        string responseStr = file.ReadToEnd();
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
                        response.ContentLength64 = buffer.Length;
                        Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }
                  
                }
                else
                {
                    response.StatusCode = 501;
                    return;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (context != null)
                {
                    context.Response.OutputStream.Close();
                }
            }
        }

        private string GetPath(String path)
        {
            if (path.IndexOf("/") == 0)
            {
                path = path.Substring(1);
            }

            if (path == "")
                path = mainPage;

            if (serverDirectory == null)
            {
                return Path.GetFullPath(path);
            }
            else
            {
                return serverDirectory + path;
            }
        }

        private void SetLastModified(HttpListenerResponse response)
        {
            response.AppendHeader("Last-modified", DateTime.Now.ToString()); 
        }
    }
}
