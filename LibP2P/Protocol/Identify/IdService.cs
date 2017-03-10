using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Crypto;
using LibP2P.Host;
using LibP2P.IO;
using LibP2P.Net;
using LibP2P.Peer;
using LibP2P.Peer.Store;
using LibP2P.Utilities.Extensions;
using Multiformats.Address;
using Multiformats.Address.Protocols;
using Multiformats.Stream;
using ProtoBuf;
using Semver;

namespace LibP2P.Protocol.Identify
{
    public class IdService : INotifiee
    {
        public static readonly ProtocolId Id = new ProtocolId("/ipfs/id/1.0.0");
        public static readonly string LibP2PVersion = "ipfs/0.1.0";
        public static readonly string ClientVersion = "cs-libp2p/0.0.1"; //TODO: pull in actual version

        public IHost Host { get; }
        // metrics

        public Multiaddress[] OwnObservedAddresses => _observedAddresses.Addresses;

        private readonly ConcurrentDictionary<INetworkConnection, TaskCompletionSource<bool>> _current;
        private readonly ObservedAddressCollection _observedAddresses;

        public IdService(IHost host)
        {
            Host = host;
            _current = new ConcurrentDictionary<INetworkConnection, TaskCompletionSource<bool>>();
            Host.SetStreamHandler(Id, RequestHandler);
        }

        public void IdentifyConnection(INetworkConnection connection)
        {
            TaskCompletionSource<bool> tcs;
            if (_current.TryGetValue(connection, out tcs))
            {
                tcs.Task.Wait();
                return;
            }

            tcs = new TaskCompletionSource<bool>();
            _current.TryAdd(connection, tcs);

            try
            {
                using (var s = connection.NewStream())
                {
                    s.Protocol = Id;
                    //wrap reporter

                    MultistreamMuxer.SelectProtoOrFail(Id, ((IReadWriter)s).AsSystemStream());

                    ResponseHandler(s);

                    _current.TryRemove(connection, out tcs);
                }
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
            finally
            {
                tcs.TrySetResult(true);
            }
        }

        public void RequestHandler(INetworkStream stream)
        {
            using (stream)
            {
                // wrap reporter

                var message = new IdentifyContract
                {
                    Protocols = Host.Mux.Protocols,
                    ObservedAddress = stream.Connection.RemoteMultiaddress.ToBytes(),
                    ListenAddresses = Host.Addresses.Select(laddr => laddr.ToBytes()).ToArray(),
                    ProtocolVersion = LibP2PVersion,
                    AgentVersion = ClientVersion
                };
                var ownKey = Host.Peerstore.PublicKey(Host.Id);
                if (ownKey == null)
                    Debug.WriteLine("Did not have own public key in Peerstore");
                else
                {
                    message.PublicKey = ownKey.Bytes;
                }

                Serializer.SerializeWithLengthPrefix(((IWriter)stream).AsSystemStream(), message, PrefixStyle.Fixed32BigEndian);
            }
        }

        public void ResponseHandler(INetworkStream stream)
        {
            using (stream)
            {
                var message = Serializer.Deserialize<IdentifyContract>(((IReader) stream).AsSystemStream());
                if (message == null)
                    return;

                ConsumeMessage(message, stream.Connection);
            }
        }

        public Task IdentifyWait(INetworkConnection connection)
        {
            TaskCompletionSource<bool> tcs;
            return _current.TryGetValue(connection, out tcs) ? tcs.Task : Task.CompletedTask;
        }

        private void ConsumeMessage(IdentifyContract message, INetworkConnection connection)
        {
            var p = connection.RemotePeer;
            Host.Peerstore.AddProtocols(p, message.Protocols);
            ConsumeObservedAddress(message.ObservedAddress, connection);

            var lmaddrs = message.ListenAddresses.Select(Multiaddress.Decode).ToArray();

            if (HasConsistentTransport(connection.RemoteMultiaddress, lmaddrs))
                lmaddrs = lmaddrs.Append(connection.RemoteMultiaddress);

            Host.Peerstore.SetAddresses(p, lmaddrs, AddressManager.ConnectedAddrTTL);

            if (!ProtocolVersionsAreCompatible(message.ProtocolVersion, LibP2PVersion))
            {
                connection.Dispose();
                return;
            }

            Host.Peerstore.Put(p, "ProtocolVersion", message.ProtocolVersion);
            Host.Peerstore.Put(p, "AgentVersion", message.AgentVersion);

            ConsumeReceivedPublicKey(connection, message.PublicKey);
        }

        private void ConsumeObservedAddress(byte[] observed, INetworkConnection connection)
        {
            if (observed == null)
                return;

            var maddr = Multiaddress.Decode(observed);
            if (maddr == null)
                return;

            var ifaceaddrs = Host.Network.InterfaceListenAddresses;
            if (ifaceaddrs == null)
                return;

            if (!ifaceaddrs.Contains(connection.LocalMultiaddress))
                return;

            _observedAddresses.Add(maddr, connection.RemoteMultiaddress);
        }

        private void ConsumeReceivedPublicKey(INetworkConnection connection, byte[] publicKey)
        {
            if (publicKey == null)
                return;

            var newKey = PublicKey.Unmarshal(publicKey);
            if (newKey == null)
                return;

            var np = new PeerId(newKey);
            if (np == null)
                return;

            if (!np.Equals(connection.RemotePeer))
            {
                if (connection.RemotePeer == "" && np != "")
                    Host.Peerstore.AddPublicKey(connection.RemotePeer, newKey);

                return;
            }

            var currentKey = Host.Peerstore.PublicKey(connection.RemotePeer);
            if (currentKey == null)
            {
                Host.Peerstore.AddPublicKey(connection.RemotePeer, newKey);
                return;
            }

            if (currentKey.Equals(newKey))
                return;

            var cp = new PeerId(currentKey);
            if (cp == null)
                return;

            if (!cp.Equals(connection.RemotePeer))
                return;
        }

        public static bool HasConsistentTransport(Multiaddress address, Multiaddress[] green)
        {
            var protos = address.Protocols.ToArray();
            return green.Any(ga => ProtocolsMatch(protos, ga.Protocols.ToArray()));
        }

        private static bool ProtocolsMatch(MultiaddressProtocol[] a, MultiaddressProtocol[] b)
        {
            if (a.Length != b.Length)
                return false;

            return !a.Where((t, i) => b[i].Code != t.Code).Any();
        }

        private static bool ProtocolVersionsAreCompatible(string v1, string v2)
        {
            if (v1.StartsWith("ipfs/"))
                v1 = v1.Substring(5);

            if (v2.StartsWith("ipfs/"))
                v2 = v2.Substring(5);

            SemVersion v1s;
            if (!SemVersion.TryParse(v1, out v1s))
                return false;

            SemVersion v2s;
            if (!SemVersion.TryParse(v2, out v2s))
                return false;

            return v1s.Major == v2s.Major && v1s.Minor == v2s.Minor;
        }

        public void Listen(INetwork network, Multiaddress ma)
        {
        }

        public void ListenClose(INetwork network, Multiaddress ma)
        {
        }

        public void Connected(INetwork network, INetworkConnection connection)
        {
        }

        public void Disconnected(INetwork network, INetworkConnection connection)
        {
            var addrs = Host.Peerstore.Addresses(connection.RemotePeer);
            Host.Peerstore.SetAddresses(connection.RemotePeer, addrs, AddressManager.RecentlyConnectedAddrTTL);
        }

        public void OpenedStream(INetwork network, INetworkStream stream)
        {
        }

        public void ClosedStream(INetwork network, INetworkStream stream)
        {
        }
    }
}
