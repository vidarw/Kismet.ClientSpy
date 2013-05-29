using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kismet.ClientSpy.Client.Classes
{
    public class WirelessClient
    {
        public string Mac { get; set; }
        public string Bssid { get; set; }

        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public int Signal { get; set; }

        public string Owner { get; set; }
    }
}
