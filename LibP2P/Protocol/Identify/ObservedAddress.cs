using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Multiformats.Address;

namespace LibP2P.Protocol.Identify
{
    public class ObservedAddress
    {
        public Multiaddress Address { get; }
        public HashSet<string> SeenBy { get; set; }
        public DateTime? LastSeen { get; set; }

        public ObservedAddress(Multiaddress address, HashSet<string> seenBy, DateTime? lastSeen = null)
        {
            Address = address;
            SeenBy = seenBy;
            LastSeen = lastSeen;
        }
    }
}
