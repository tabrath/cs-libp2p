using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Net;
using LibP2P.Peer;
using LibP2P.Peer.Store;
using LibP2P.Protocol;
using Multiformats.Address;
using Multiformats.Stream;

namespace LibP2P.Host
{
    public interface IHost : IDisposable
    {
        PeerId Id { get; }
        PeerStore Peerstore { get; }
        Multiaddress[] Addresses { get; }
        INetwork Network { get; }
        MultistreamMuxer Mux { get; }

        Task Connect(PeerInfo pi, CancellationToken cancellationToken);
        void SetStreamHandler(ProtocolId pid, StreamHandler handler);
        void SetStreamHandlerMatch(ProtocolId pid, Func<string, bool> match, StreamHandler handler);
        void RemoveStreamHandler(ProtocolId pid);
        Task<INetworkStream> NewStream(PeerId peer, IEnumerable<ProtocolId> protocols, CancellationToken cancellationToken);
    }
}
