using HttpWebServer.BL;
using HttpWebServer.DAL;
using HttpWebServer.Infrastructure;
using HttpWebServer.Models;
using System;
using System.Net;

namespace HttpWebServer.Contoller
{
    class ParticipantsController : BaseController
    {
        public DateTime DateModificat { get; set; }

        public ParticipantsController(IParticipantsService participantsService, ILogger logger, DateTime dateModificat) : base(participantsService, logger)
        {
            DateModificat = dateModificat;
        }
        public override void Handle(HttpListenerContext context)
        {
            Logger.Info(context.Request.Url.ToString());

            if (context.Request.HttpMethod.Equals("GET"))
            {
                string responseStr="";

                if (context.Request.Url.AbsolutePath == "/participants_list")
                {
                    responseStr = $"<ol>{ParticipantsHTML.HTMLView(ParticipantsService)}</ol>";
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

                    string participantsFilePath = GetPath(ParticipantsRepository.participantsFile);
                    responseStr = ParticipantsHTML.Replace(ParticipantsService, responseStr);
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
