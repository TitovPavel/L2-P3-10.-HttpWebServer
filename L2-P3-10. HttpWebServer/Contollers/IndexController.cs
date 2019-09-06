using HttpWebServer.BL;
using HttpWebServer.Infrastructure;
using System.Net;

namespace HttpWebServer.Contoller
{
    class IndexController : BaseController
    {
        public IndexController(IParticipantsService participantsService, ILogger logger) : base(participantsService, logger)
        {

        }
        public override void Handle(HttpListenerContext context)
        {
            Logger.Info(context.Request.Url.ToString());
            if (context.Request.HttpMethod.Equals("GET"))
            {
                string filePath = GetPath("index.html");
                string responseStr = GetResponse(filePath);
                Send(context, responseStr);
            }
            else
            {
                context.Response.StatusCode = 501;
            }
        }
    }
}
