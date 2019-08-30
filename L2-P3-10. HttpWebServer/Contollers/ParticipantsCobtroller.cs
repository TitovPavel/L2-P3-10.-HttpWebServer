using L2_P3_10.HttpWebServer.Models;
using System;
using System.Net;

namespace L2_P3_10.HttpWebServer.Contoller
{
    class ParticipantsController : BaseController
    {
        private DateTime DateModificat { get; set; }

        public ParticipantsController(string serverDirectory, DateTime dateModificat) : base(serverDirectory)
        {
            DateModificat = dateModificat;
        }
        public override void Handle(HttpListenerContext context)
        {

            if (context.Request.HttpMethod.Equals("GET"))
            {
                string responseStr;

                if (context.Request.Url.AbsolutePath == "/participants_list")
                {
                    string participantsFilePath = GetPath(Participants.participantsFile);
                    responseStr = $"<ol>{Participants.HTMLView(participantsFilePath)}</ol>";
                }
                else
                {
                    string ifModifiedSince = context.Request.Headers.Get("if-Modified-Since");
                    DateTime dateTimeModified;

                    if (ifModifiedSince == null || (DateTime.TryParse(ifModifiedSince, out dateTimeModified) && (dateTimeModified.AddMinutes(2) < DateTime.Now || DateModificat > dateTimeModified)))
                    {
                        SetLastModified(context.Response);
                    }
                    else
                    {
                        context.Response.StatusCode = 304;
                        return;
                    }

                    string filePath = GetPath("participants.html");
                    responseStr = GetResponse(filePath);

                    string participantsFilePath = GetPath(Participants.participantsFile);
                    responseStr = Participants.Replace(participantsFilePath, responseStr);
                }

                Send(context, responseStr);
            }
            else
            {
                context.Response.StatusCode = 501;
            }
        }

        private void SetLastModified(HttpListenerResponse response)
        {
            response.AppendHeader("Last-modified", DateTime.Now.ToString());
        }
    }
}
