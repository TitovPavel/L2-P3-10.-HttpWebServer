using HttpWebServer.BL;
using HttpWebServer.Contoller;
using HttpWebServer.Infrastructure;
using HttpWebServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace HttpWebServer
{
    class Program
    {
        public static string serverDirectory;

        static void Main(string[] args)
        {
            Server ws = new Server("http://localhost:8881/");
            ws.RunAsync();

            Console.WriteLine("Press a key to quit.");
            Console.ReadKey();

            ws.Stop();
        }
    }

    class Server: IDisposable
    {
        private HttpListener listener;
        private List<WebSocket> clients = new List<WebSocket>();

        private DateTime dateModificat = DateTime.Now;
        readonly ILogger logger;
        readonly IParticipantsService participantsService;
        readonly Locator container = new Locator();

        public Server(string URI)
        {
            ReadSettings();
            container.Register();
            participantsService = container.Resolve(typeof(IParticipantsService)) as IParticipantsService;
            logger = container.Resolve(typeof(ILogger)) as ILogger;
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
                        Program.serverDirectory = line.Replace("serverDirectory=", "");
                    }
                }
            }
        }

        public async void RunAsync()
        {
            Console.WriteLine("Webserver running...");

            while (listener.IsListening)
            {
                HttpListenerContext context = await listener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    WriteToWebSocketAsync(context);
                }
                else
                {
                    WriteToClient(context);
                }
            }

        }

        public void Stop()
        {
            this.Dispose();
        }


        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }
                listener.Stop();
                listener.Close();

                disposed = true;
            }
        }

        ~Server()
        {
            Dispose(false);
        }

        private async void WriteToWebSocketAsync(HttpListenerContext context)
        {

            HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;

            clients.Add(webSocket);

            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<Byte>(new byte[1024]);
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                for (int i = 0; i < clients.Count; i++)
                {
                    WebSocket client = clients[i];
                    try
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }

                    catch (ObjectDisposedException)
                    {
                        try
                        {
                            clients.Remove(client);
                            i--;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        private void WriteToClient(HttpListenerContext context)
        {
            try
            {
                BaseController targetController;
                HttpListenerRequest request = context.Request;

                if (request.Url.AbsolutePath == "/vote.html" || request.Url.AbsolutePath == "/vote")
                {
                    if (request.Url.AbsolutePath == "/vote")
                    {
                        dateModificat = DateTime.Now;

                    }
                    targetController = container.Resolve("Vote") as BaseController;
                }
                else if (request.Url.AbsolutePath == "/participants.html" || request.Url.AbsolutePath == "/participants_list")
                {
                    targetController = new ParticipantsController(participantsService, logger, dateModificat);

                }
                else if (request.Url.AbsolutePath == "/index.html" || request.Url.AbsolutePath == "/")
                {
                    targetController = container.Resolve("Index") as BaseController;
                }
                else
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                targetController.Handle(context);

            }
            catch (Exception ex)
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
    }   
}
