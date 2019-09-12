using System.Collections.Generic;
using HttpWebServer.BL;

namespace HttpWebServer.Models
{
    class ParticipantsHTML
    {
        public static string HTMLView(IParticipantsService participantsService)
        {
            string HTML = "";
            List<Participant> listParticipants = participantsService.ListAttendent();
            foreach (Participant p in listParticipants)
            {
                HTML = HTML + "<li>" + p.Name + "</li>";
            }
            return HTML;
        }
        public static string Replace(IParticipantsService participantsService, string responseStr)
        {
            if (responseStr.Contains("{{participants}}"))
            {
                string participantsHTML = HTMLView(participantsService);
                responseStr = responseStr.Replace("{{participants}}", participantsHTML);
            }
            return responseStr;

        }
    }
}
