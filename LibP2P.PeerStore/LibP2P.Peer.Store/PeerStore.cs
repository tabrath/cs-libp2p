using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Multiformats.Address;
using LibP2P.Crypto;

namespace LibP2P.Peer.Store
{
    public class PeerStore : IKeyBook, IMetrics, IAddressManager
    {
        public static readonly TimeSpan AddressTTL = TimeSpan.FromHours(1);

        private readonly ConcurrentDictionary<string, object> _ds;
        private readonly IKeyBook _keybook;
        private readonly IMetrics _metrics;
        private readonly IAddressManager _addrManager;

        public PeerId[] Peers => _keybook.Peers.Concat(_addrManager.Peers).ToArray();

        public PeerStore(IKeyBook keyBook = null, IMetrics metrics = null)
        {
            _keybook = keyBook ?? new KeyBook();
            _metrics = metrics ?? new Metrics();
            _addrManager = new AddressManager();
            _ds = new ConcurrentDictionary<string, object>();
        }

        public void AddPrivateKey(PeerId p, PrivateKey sk) => _keybook.AddPrivateKey(p, sk);
        public void AddPublicKey(PeerId p, PublicKey pk) => _keybook.AddPublicKey(p, pk);
        public PrivateKey PrivateKey(PeerId p) => _keybook.PrivateKey(p);
        public PublicKey PublicKey(PeerId p) => _keybook.PublicKey(p);

        public PeerInfo PeerInfo(PeerId id) => new PeerInfo(id, _addrManager.Addresses(id));

        public object Get(PeerId id, string key)
        {
            object value;
            return _ds.TryGetValue($"{id}/{key}", out value) ? value : null;
        }

        public void Put(PeerId id, string key, object value)
        {
            _ds.AddOrUpdate($"{id}/{key}", value, (x,y) => value);
        }

        private IDictionary<string, object> GetProtocolMap(PeerId p)
        {
            var map = (IDictionary< string, object>)Get(p, "protocols");
            if (map == null)
                map = new Dictionary<string, object>();

            return map;
        }

        public string[] GetProtocols(PeerId id) => GetProtocolMap(id).Keys.ToArray();

        public void AddProtocols(PeerId id, params string[] protocols)
        {
            var map = GetProtocolMap(id);

            foreach (var protocol in protocols)
            {
                map[protocol] = new object();
            }

            Put(id, "protocols", map);
        }

        public string[] SupportsProtocols(PeerId id, params string[] protocols)
        {
            var map = GetProtocolMap(id);
            return protocols.Where(p => map.ContainsKey(p)).ToArray();
        }

        public PeerInfo[] PeerInfos(params PeerId[] peers) => peers.Select(PeerInfo).ToArray();
        public PeerId[] PeerInfoIds(params PeerInfo[] pis) => pis.Select(p => p.Id).ToArray();

        public void RecordLatency(string peerId, TimeSpan duration) => _metrics.RecordLatency(peerId, duration);
        public TimeSpan LatencyEWMA(string peerId) => _metrics.LatencyEWMA(peerId);

        public Multiaddress[] Addresses(PeerId p) => _addrManager.Addresses(p);
        public void AddAddress(PeerId p, Multiaddress addr, TimeSpan ttl) => _addrManager.AddAddress(p, addr, ttl);
        public void AddAddresses(PeerId p, IEnumerable<Multiaddress> addrs, TimeSpan ttl) => _addrManager.AddAddresses(p, addrs, ttl);
        public void ClearAddresses(PeerId peer) => _addrManager.ClearAddresses(peer);
        //public BlockingCollection<Multiaddress> AddressStream(PeerId peer, CancellationToken cancellationToken) => _addrManager.AddressStream(peer, cancellationToken);
        public void SetAddress(PeerId p, Multiaddress addr, TimeSpan ttl) => _addrManager.SetAddress(p, addr, ttl);
        public void SetAddresses(PeerId p, IEnumerable<Multiaddress> addrs, TimeSpan ttl) => _addrManager.SetAddresses(p, addrs, ttl);
    }
}