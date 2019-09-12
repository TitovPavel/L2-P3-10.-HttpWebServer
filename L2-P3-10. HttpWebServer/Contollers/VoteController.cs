using HttpWebServer.BL;
using HttpWebServer.Infrastructure;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;

namespace HttpWebServer.Contoller
{
    class VoteController : BaseController
    {
        public VoteController(IParticipantsService participantsService, ILogger logger) :base(participantsService, logger)
        {

        }
        public override void Handle(HttpListenerContext context)
        {
            Logger.Info(context.Request.Url.ToString());
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
                    StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                    string s = reader.ReadToEnd();

                    NameValueCollection query = HttpUtility.ParseQueryString(s);

                    if (query["attend"] == "on")
                    {
                        ParticipantsService.Vote(query["name"], true);
                    }
                    else 
                    {
                        ParticipantsService.Vote(query["name"], false);
                    }

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
