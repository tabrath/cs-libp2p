using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Net;
using LibP2P.Peer;
using LibP2P.Peer.Store;
using LibP2P.Protocol;
using LibP2P.Routing;
using Multiformats.Address;
using Multiformats.Stream;

namespace LibP2P.Host.Routed
{
    public class RoutedHost : IHost
    {
        public static readonly TimeSpan AddressTTL = TimeSpan.FromSeconds(10);

        public PeerId Id => _host.Id;
        public PeerStore Peerstore => _host.Peerstore;
        public Multiaddress[] Addresses => _host.Addresses;
        public INetwork Network => _host.Network;
        public MultistreamMuxer Mux => _host.Mux;

        private readonly IHost _host;
        private readonly IPeerRouting _route;

        protected RoutedHost(IHost host, IPeerRouting route)
        {
            _host = host;
            _route = route;
        }

        public static RoutedHost Wrap(IHost host, IPeerRouting route) => new RoutedHost(host, route);

        public async Task Connect(PeerInfo pi, CancellationToken cancellationToken)
        {
            if (Network.ConnectionsToPeer(pi.Id).Length > 0)
                return;

            if (pi.Addresses.Length > 0)
                Peerstore.AddAddresses(pi.Id, pi.Addresses, AddressManager.TempAddrTTL);

            var addrs = Peerstore.Addresses(pi.Id);
            if (addrs.Length < 1)
            {
                var pi2 = await _route.FindPeer(pi.Id, cancellationToken);
                if (pi2 == null)
                    return;

                if (pi2.Id != pi.Id)
                    return;

                addrs = pi2.Addresses;
            }

            //TODO: make this accessible in PeerStore
            pi.Addresses = addrs;

            await _host.Connect(pi, cancellationToken);
        }

        public void SetStreamHandler(ProtocolId pid, StreamHandler handler) => _host.SetStreamHandler(pid, handler);
        public void SetStreamHandlerMatch(ProtocolId pid, Func<string, bool> match, StreamHandler handler) => _host.SetStreamHandlerMatch(pid, match, handler);
        public void RemoveStreamHandler(ProtocolId pid) => _host.RemoveStreamHandler(pid);
        public Task<INetworkStream> NewStream(PeerId peer, IEnumerable<ProtocolId> protocols, CancellationToken cancellationToken) => _host.NewStream(peer, protocols, cancellationToken);
        public void Dispose() => _host.Dispose();
    }
}
