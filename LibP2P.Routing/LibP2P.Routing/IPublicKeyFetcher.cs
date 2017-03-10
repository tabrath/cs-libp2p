using System.Threading;
using System.Threading.Tasks;
using LibP2P.Crypto;
using LibP2P.Peer;

namespace LibP2P.Routing
{
    public interface IPublicKeyFetcher
    {
        Task<PublicKey> GetPublicKey(PeerId peer, CancellationToken cancellationToken);
    }
}