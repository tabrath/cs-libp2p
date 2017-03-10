using System.Collections.Generic;
using System.Linq;
using LibP2P.Peer;
using LibP2P.Utilities.Extensions;

namespace LibP2P.KBucket
{
    internal class Util
    {
        public class PeerDistance
        {
            public PeerId P { get; set; }
            public DhtId Distance { get; set; }
        }

        public static IEnumerable<PeerDistance> CopyPeersFromList(DhtId target, IEnumerable<PeerId> peers)
        {
            return from peer in peers let pid = DhtId.ConvertPeerId(peer) select new PeerDistance { P = peer, Distance = ((byte[])target).XOR(pid) };
        }

        public static IEnumerable<PeerId> SortClosestPeers(IEnumerable<PeerId> peers, DhtId target)
        {
            return CopyPeersFromList(target, peers)
                .OrderBy(p => p.Distance, new DistanceComparer())
                .Select(p => p.P);
        }

        internal class DistanceComparer : IComparer<DhtId>
        {
            public int Compare(DhtId x, DhtId y) => x.CompareTo(y);
        }
    }
}
