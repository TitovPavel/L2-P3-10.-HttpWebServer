using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace L2_P3_10.HttpWebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server ws = new Server("http://*:8881/");
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


        private string serverDirectory;

        const string mainPage = "index.html";
        const string participantsFile = "Participants.json";

        DateTime dateModificat = DateTime.Now;


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

        public async void RunAsync()
        {
            Console.WriteLine("Webserver running...");

            while (listener.IsListening)
            {
                WriteToClient(await listener.GetContextAsync());
            }

        }

        public void Stop()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            listener.Stop();
            listener.Close();
        }



        private async void WriteToClient(HttpListenerContext context)
        {

            try
            {
                if (context == null)
                {
                    return;
                }

                HttpListenerResponse response = context.Response;
                HttpListenerRequest request = context.Request;

                if (request.IsWebSocketRequest)
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
                                    Console.WriteLine(ex.StackTrace);
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (context.Request.HttpMethod.Equals("GET"))
                    {
                        if (request.Url.AbsolutePath == "/participants_list")
                        {
                            string responseStr = $"<ol>{Participants.HTMLView(participantsFile)}</ol>";
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
                            response.ContentLength64 = buffer.Length;
                            Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                        }
                        else
                        {
                            string filePath = GetPath(request.Url.AbsolutePath);
                            if (!File.Exists(filePath))
                            {
                                response.StatusCode = 404;
                                return;
                            }

                            string ifModifiedSince = request.Headers.Get("if-Modified-Since");
                            DateTime dateTimeModified;

                            if (ifModifiedSince == null || (DateTime.TryParse(ifModifiedSince, out dateTimeModified) && (dateTimeModified.AddMinutes(2) < DateTime.Now|| dateModificat > dateTimeModified)))
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

                                responseStr = Participants.Replace(participantsFile, responseStr);

                                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
                                response.ContentLength64 = buffer.Length;
                                Stream output = response.OutputStream;
                                output.Write(buffer, 0, buffer.Length);
                                output.Close();
                            }
                        }
                    }

                    else if (context.Request.HttpMethod.Equals("POST"))
                    {
                        if (request.Url.AbsolutePath == "/vote")
                        {
                            Participants.Add(participantsFile, request);
                            dateModificat = DateTime.Now;
                            response.Redirect("participants.html");
                        }
                        else
                        {
                            response.StatusCode = 404;
                            return;
                        }
                    }
                    else
                    {
                        response.StatusCode = 501;
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
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

    [DataContract]
    class Participants
    {
        [DataMember]
        public List<String> ListParticipants { get; set; }

        public static Participants Read(string path)
        {
            if (!File.Exists(path))
                return null;

            object p;

            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Participants));

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                p = jsonFormatter.ReadObject(fs);
            }
            return p as Participants;
        }

        public static void Write(string path, Participants p)
        {
            if (!File.Exists(path))
                return ;
        
            using (StreamWriter fs = new StreamWriter(path))
            {
                fs.Write(JsonConvert.SerializeObject(p));
            }

        }

        public static string HTMLView(string path)
        {
            string HTML = "";
            Participants participants = Participants.Read(path);
            foreach(string p in participants.ListParticipants)
            {
                HTML = HTML + "<li>" + p + "</li>";
            }
            return HTML;
        }
        public static string Replace(string path, string responseStr)
        {
            if (responseStr.Contains("{{participants}}"))
            {
                string participantsHTML = Participants.HTMLView(path);
                responseStr = responseStr.Replace("{{participants}}", participantsHTML);
            }
            return responseStr;

        }

        public static void Add(string path, HttpListenerRequest request)
        {
            StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
            string s = reader.ReadToEnd();

            NameValueCollection query = HttpUtility.ParseQueryString(s);

            if(query["attend"] == "on")
            {
                Participants participants = Participants.Read(path);
                participants.ListParticipants.Add(query["name"]);
                Write(path, participants);
            }

        }
    }
}
