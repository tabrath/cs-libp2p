using Multiformats.Address;

namespace LibP2P.Net
{
    public interface INotifiee
    {
        void Listen(INetwork network, Multiaddress ma);
        void ListenClose(INetwork network, Multiaddress ma);
        void Connected(INetwork network, INetworkConnection connection);
        void Disconnected(INetwork network, INetworkConnection connection);
        void OpenedStream(INetwork network, INetworkStream stream);
        void ClosedStream(INetwork network, INetworkStream stream);

        //TODO
        //void PeerConnected(INetwork network, PeerId peer);
        //void PeerDisconnected(INetwork network, PeerId peer);
    }
}
