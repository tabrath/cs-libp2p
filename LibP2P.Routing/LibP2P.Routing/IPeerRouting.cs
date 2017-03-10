using System.Threading;
using System.Threading.Tasks;
using LibP2P.Peer;
using LibP2P.Peer.Store;

namespace LibP2P.Routing
{
    public interface IPeerRouting
    {
        Task<PeerInfo> FindPeer(PeerId peer, CancellationToken cancellationToken);
    }
}