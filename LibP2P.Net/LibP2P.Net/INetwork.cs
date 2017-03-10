using System;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Peer;
using Multiformats.Address;

namespace LibP2P.Net
{
    public interface INetwork : INetworkDialer, IDisposable
    {
        Multiaddress[] ListenAddresses { get; }
        Multiaddress[] InterfaceListenAddresses { get; }

        void SetStreamHandler(StreamHandler handler);
        void SetConnectionHandler(ConnectionHandler handler);
        Task<INetworkStream> NewStreamAsync(PeerId peer, CancellationToken cancellationToken);
        void Listen(params Multiaddress[] addresses);
        void Notify(INotifiee notifiee);
        void StopNotify(INotifiee notifiee);
    }
}