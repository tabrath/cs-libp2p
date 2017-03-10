using System.Threading;
using System.Threading.Tasks;
using LibP2P.Peer;
using LibP2P.Peer.Store;

namespace LibP2P.Net
{
    public interface INetworkDialer
    {
        PeerStore Peerstore { get; }
        PeerId LocalPeer { get; }
        PeerId[] Peers { get; }
        INetworkConnection[] Connections { get; }

        Task<INetworkConnection> DialPeerAsync(PeerId peer, CancellationToken cancellationToken);
        void ClosePeer(PeerId peer);
        Connectedness Connectedness(PeerId peer);
        INetworkConnection[] ConnectionsToPeer(PeerId peer);
    }
}