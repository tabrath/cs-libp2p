using LibP2P.Crypto;
using LibP2P.Peer;

namespace LibP2P.Abstractions.Connection
{
    public interface IPeerConnection : IConnection
    {
        string Id { get; }

        PeerId LocalPeer { get; }
        PrivateKey LocalPrivateKey { get; }

        PeerId RemotePeer { get; }
        PublicKey RemotePublicKey { get; }
    }
}