using HttpWebServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace HttpWebServer.DAL
{
    class ParticipantsRepository : IParticipantsRepository
    {
        public const string participantsFile = "Participants.json";
        string path;

        List<Participant> participants;


        public ParticipantsRepository()
        {
            path = Program.serverDirectory + participantsFile;
        }

    

        public List<Participant> List()
        {
            if (!File.Exists(path))
                return null;

            if (participants == null)
            {
                using (StreamReader file = new StreamReader(path))
                {
                    String participantsString = file.ReadToEnd();
                    participants = JsonConvert.DeserializeObject(participantsString, typeof(List<Participant>)) as List<Participant>;
                }
            }
            return participants;
        }

        public void Save(List<Participant> p)
        {
            if (!File.Exists(path))
                return;

            using (StreamWriter fs = new StreamWriter(path))
            {
                fs.Write(JsonConvert.SerializeObject(p));
            }

        }

        public void Delete(string name)
        {
            participants.RemoveAll(x => x.Name == name);
            Save(participants);
        }
    }
}
