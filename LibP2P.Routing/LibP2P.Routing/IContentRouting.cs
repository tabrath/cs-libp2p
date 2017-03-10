using System;
using System.Threading;
using System.Threading.Tasks;
using ContentIdentifier;
using LibP2P.Peer.Store;

namespace LibP2P.Routing
{
    public interface IContentRouting
    {
        Task Provide(Cid cid, CancellationToken cancellationToken);
        Task<Action<PeerInfo, bool>> FindProvidersAsync(Cid cid, CancellationToken cancellationToken);
    }
}
