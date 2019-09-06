using HttpWebServer.Models;
using System.Collections.Generic;

namespace HttpWebServer.DAL
{
    interface IParticipantsRepository
    {
        void Delete(string name);
        List<Participant> List();
        void Save(List<Participant> p);
    }
}