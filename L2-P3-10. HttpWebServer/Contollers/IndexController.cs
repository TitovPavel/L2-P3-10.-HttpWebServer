using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace L2_P3_10.HttpWebServer.Contoller
{
    class IndexController : BaseController
    {
        public IndexController(string serverDirectory) : base(serverDirectory)
        {

        }
        public override void Handle(HttpListenerContext context)
        {
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
