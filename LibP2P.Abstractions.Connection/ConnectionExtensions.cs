using System.Text;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;
using LibP2P.Peer;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Abstractions.Connection
{
    public static class ConnectionExtensions
    {
        public static string GetId(this IPeerConnection c)
        {
            var local = $"{c.LocalMultiaddress}/{c.LocalPeer}";
            var remote = $"{c.RemoteMultiaddress}/{c.RemotePeer}";
            var localHash = (byte[])Multihash.Sum<SHA2_256>(Encoding.UTF8.GetBytes(local));
            var remoteHash = (byte[])Multihash.Sum<SHA2_256>(Encoding.UTF8.GetBytes(remote));
            var ch = localHash.XOR(remoteHash);

            return new PeerId(ch).ToString();
        }

        public static string GetString(this IPeerConnection c, string type) => $"{c.LocalPeer} ({c.LocalMultiaddress}) <-- {type} {c} --> ({c.RemoteMultiaddress}) {c.RemotePeer}";
  }
}