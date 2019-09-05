using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;

namespace L2_P3_10.HttpWebServer.Models
{
    class Participants
    {
        public const string participantsFile = "Participants.json";

        public static ParticipantsView Read(string path)
        {
            if (!File.Exists(path))
                return null;

            object p;

            using (StreamReader file = new StreamReader(path))
            {
                String participantsString = file.ReadToEnd();
                p = JsonConvert.DeserializeObject(participantsString, typeof(ParticipantsView));
            }
            return p as ParticipantsView;
        }

        public static void Write(string path, ParticipantsView p)
        {
            if (!File.Exists(path))
                return;

            using (StreamWriter fs = new StreamWriter(path))
            {
                fs.Write(JsonConvert.SerializeObject(p));
            }

        }

        public static string HTMLView(string path)
        {
            string HTML = "";
            if (File.Exists(path))
            {
                ParticipantsView participants = Read(path);
                foreach (string p in participants.ListParticipants)
                {
                    HTML = HTML + "<li>" + p + "</li>";
                }
            }
            return HTML;
        }
        public static string Replace(string path, string responseStr)
        {
            if (File.Exists(path))
            {
                if (responseStr.Contains("{{participants}}"))
                {
                    string participantsHTML = HTMLView(path);
                    responseStr = responseStr.Replace("{{participants}}", participantsHTML);
                }
            }
            return responseStr;

        }

        public static void Add(string path, HttpListenerRequest request)
        {
            StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
            string s = reader.ReadToEnd();

            NameValueCollection query = HttpUtility.ParseQueryString(s);

            if (query["attend"] == "on")
            {
                ParticipantsView participants = Read(path);
                participants.ListParticipants.Add(query["name"]);
                Write(path, participants);
            }

        }
    }
}
