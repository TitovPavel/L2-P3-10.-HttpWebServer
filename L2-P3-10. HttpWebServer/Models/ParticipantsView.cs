using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace L2_P3_10.HttpWebServer.Models
{
    [DataContract]
    class ParticipantsView
    {
        [DataMember]
        public List<String> ListParticipants { get; set; }

    }
}
