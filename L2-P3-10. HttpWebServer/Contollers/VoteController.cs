using L2_P3_10.HttpWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L2_P3_10.HttpWebServer.Contoller
{
    class VoteController : BaseController
    {
        public VoteController(string serverDirectory) :base(serverDirectory)
        {

        }
        public override void Handle(HttpListenerContext context)
        {
            if (context.Request.HttpMethod.Equals("GET"))
            {
                string filePath = GetPath("vote.html");
                string responseStr = GetResponse(filePath);
                Send(context, responseStr);
            }
            else if (context.Request.HttpMethod.Equals("POST"))
            {
                if (context.Request.Url.AbsolutePath == "/vote")
                {
                    string participantsFilePath = GetPath(Participants.participantsFile);
                    Participants.Add(participantsFilePath, context.Request);
                    context.Response.Redirect("participants.html");
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            }
            else
            {
                context.Response.StatusCode = 501;
            }
        }
    }
}
