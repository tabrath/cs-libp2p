using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.IO;
using LibP2P.Net;
using LibP2P.Peer;
using LibP2P.Peer.Store;
using LibP2P.Protocol;
using LibP2P.Protocol.Identify;
using LibP2P.Utilities.Extensions;
using Multiformats.Address;
using Multiformats.Stream;

namespace LibP2P.Host.Basic
{
    public class BasicHost : IHost
    {
        public PeerId Id => Network.LocalPeer;
        public PeerStore Peerstore => Network.Peerstore;

        public Multiaddress[] Addresses
        {
            get
            {
                var addrs = Network.InterfaceListenAddresses;

                if (_ids != null)
                    addrs = addrs.Append(_ids.OwnObservedAddresses);

                // natmngr
                if (_nat != null)
                {
                }

                return addrs;
            }
        }
        public INetwork Network { get; }
        public MultistreamMuxer Mux { get; }

        public static TimeSpan NegotiateTimeout { get; set; }

        private readonly IdService _ids;
        private readonly NatManager _nat;

        public BasicHost(INetwork network, BasicHostOptions options = 0)
        {
            Network = network;
            Mux = new MultistreamMuxer();

            _ids = new IdService(this);

            var handle = new Action<INetworkStream>(s => Mux.Handle(((IReadWriteCloser)s).AsSystemStream()));
            //relay service
            //reporter

            if (options.HasFlag(BasicHostOptions.NatPortMap))
                _nat = new NatManager(this);

            network.SetConnectionHandler(NewConnectionHandler);
            network.SetStreamHandler(NewStreamHandler);
        }

        private void NewStreamHandler(INetworkStream stream)
        {
            var sw = new Stopwatch();
            sw.Start();

            if (NegotiateTimeout != Timeout.InfiniteTimeSpan)
                stream.SetDeadline(DateTime.Now.Add(NegotiateTimeout));

            var result = Mux.Negotiate(((IReadWriter)stream).AsSystemStream());
            sw.Stop();
            
            if (NegotiateTimeout != Timeout.InfiniteTimeSpan)
                stream.SetDeadline(default(DateTime));

            stream.Protocol = result.Protocol;

            // reporter

            Task.Factory.StartNew(() => result.Handler.Handle(result.Protocol, ((IReadWriter)stream).AsSystemStream()));
        }

        private void NewConnectionHandler(INetworkConnection connection)
        {
            Peerstore.AddProtocols(connection.RemotePeer);
            _ids.IdentifyConnection(connection);
        }

        public void Dispose()
        {
            _nat?.Dispose();
        }

        public Task Connect(PeerInfo pi, CancellationToken cancellationToken)
        {
            Peerstore.AddAddresses(pi.Id, pi.Addresses, AddressManager.TempAddrTTL);

            var cs = Network.ConnectionsToPeer(pi.Id);
            if (cs.Length > 0)
                return Task.CompletedTask;

            return DialPeer(pi.Id, cancellationToken);
        }

        private async Task DialPeer(PeerId peer, CancellationToken cancellationToken)
        {
            var c = await Network.DialPeerAsync(peer, cancellationToken);
            if (c == null)
                return;

            Peerstore.AddProtocols(peer);

            await Task.Factory.StartNew(() => _ids.IdentifyConnection(c), cancellationToken);
        }

        public void SetStreamHandler(ProtocolId pid, StreamHandler handler)
        {
            Mux.AddHandler(pid, (p, rwc) =>
            {
                var ns = (INetworkStream) rwc;
                ns.Protocol = p;
                handler(ns);

                return true;
            });
        }

        public void SetStreamHandlerMatch(ProtocolId pid, Func<string, bool> match, StreamHandler handler)
        {
            /*Mux.AddHandler(pid, match, (p, rwc) =>
            {
                var ns = (INetworkStream) rwc;
                ns.Protocol = p;
                handler(ns);

                return true;
            });*/
        }

        public void RemoveStreamHandler(ProtocolId pid)
        {
            Mux.RemoveHandler((string)pid);
        }

        public async Task<INetworkStream> NewStream(PeerId peer, IEnumerable<ProtocolId> protocols, CancellationToken cancellationToken)
        {
            var pref = PreferredProtocol(peer, protocols);
            if (pref != "")
                return await NewStream(peer, pref, cancellationToken);

            var protosstrs = protocols.Select(p => p.ToString()).ToArray();

            var s = await Network.NewStreamAsync(peer, cancellationToken);
            if (s == null)
                return null;

            var selected = await MultistreamMuxer.SelectOneOfAsync(protosstrs, ((IReadWriter)s).AsSystemStream(), cancellationToken);
            if (selected == null)
            {
                s.Dispose();
                return null;
            }

            s.Protocol = new ProtocolId(selected);
            Peerstore.AddProtocols(peer, selected);

            //reporter

            return s;
        }

        private ProtocolId PreferredProtocol(PeerId peer, IEnumerable<ProtocolId> pids)
        {
            var pidstrs = pids.Select(p => p.ToString()).ToArray();
            var supported = Peerstore.SupportsProtocols(peer, pidstrs);
            if (supported == null)
                return string.Empty;

            return supported.FirstOrDefault();
        }

        private async Task<INetworkStream> NewStream(PeerId peer, ProtocolId pid, CancellationToken cancellationToken)
        {
            var s = await Network.NewStreamAsync(peer, cancellationToken);
            if (s == null)
                return null;

            s.Protocol = pid;
            //wrap reporter

            var con = Multistream.CreateSelect(((IReadWriter)s).AsSystemStream(), pid);

            return new StreamWrapper(s, con.AsReadWriter());
        }

        private class StreamWrapper : INetworkStream
        {
            private readonly INetworkStream _stream;
            private readonly IReadWriter _rw;

            public ProtocolId Protocol
            {
                get { return _stream.Protocol; }
                set { _stream.Protocol = value; }
            }

            public INetworkConnection Connection => _stream.Connection;

            public StreamWrapper(INetworkStream stream, IReadWriter rw)
            {
                _stream = stream;
                _rw = rw;
            }

            public int Read(byte[] buffer, int offset, int count) => _rw.Read(buffer, offset, count);
            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _rw.ReadAsync(buffer, offset, count, cancellationToken);
            public int Write(byte[] buffer, int offset, int count) => _rw.Write(buffer, offset, count);
            public Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _rw.WriteAsync(buffer, offset, count, cancellationToken);
            public void Dispose() => _stream.Dispose();
            public void SetDeadline(DateTime t) => _stream.SetDeadline(t);
            public void SetReadDeadline(TimeSpan t) => _stream.SetReadDeadline(t);
            public void SetWriteDeadline(TimeSpan t) => _stream.SetWriteDeadline(t);
        }
    }

    [Flags]
    public enum BasicHostOptions
    {
        NatPortMap = 0x01
    }
}
