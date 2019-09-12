using HttpWebServer.Models;
using System.Collections.Generic;

namespace HttpWebServer.BL
{
    interface IParticipantsService
    {
        List<Participant> ListAll();
        List<Participant> ListAttendent();
        List<Participant> ListMissed();
        void Vote(string name, bool attend);
    }
}