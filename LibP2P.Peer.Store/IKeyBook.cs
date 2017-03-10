using LibP2P.Crypto;

namespace LibP2P.Peer.Store
{
    public interface IKeyBook
    {
        PeerId[] Peers { get; }

        void AddPrivateKey(PeerId p, PrivateKey sk);
        void AddPublicKey(PeerId p, PublicKey pk);
        PrivateKey PrivateKey(PeerId p);
        PublicKey PublicKey(PeerId p);
    }
}