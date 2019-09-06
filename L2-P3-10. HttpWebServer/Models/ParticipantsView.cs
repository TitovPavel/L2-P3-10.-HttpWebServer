using System.Collections.Generic;

namespace HttpWebServer.Models
{
    class Participant
    {
        public Participant(string name, bool attend)
        {
            Name = name;
            Attend = attend;
        }
        public string Name { get; set; }
        public bool Attend { get; set; }
    }
}
