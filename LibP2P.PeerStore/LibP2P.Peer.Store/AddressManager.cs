using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Multiformats.Address;
using Multiformats.Address.Net;
using Multiformats.Address.Protocols;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Peer.Store
{
    public class AddressManager : IAddressManager
    {
        public static readonly TimeSpan TempAddrTTL = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan ProviderAddrTTL = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan RecentlyConnectedAddrTTL = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan OwnObservedAddrTTL = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan PermanentAddrTTL = TimeSpan.FromHours(24*356);
        public static readonly TimeSpan ConnectedAddrTTL = PermanentAddrTTL;

        private class ExpiringAddress
        {
            public Multiaddress Address { get; }
            public DateTime TTL { get; }

            public ExpiringAddress(Multiaddress address, DateTime ttl)
            {
                Address = address;
                TTL = ttl;
            }

            public bool ExpiredBy(DateTime dt) => dt > TTL;
        }

        private readonly ConcurrentDictionary<PeerId, Dictionary<string, ExpiringAddress>> _addrs;
        private readonly ConcurrentDictionary<PeerId, AddrSub[]> _addrSubs;

        public PeerId[] Peers => _addrs.Keys.Select(id => id).ToArray();

        public AddressManager()
        {
            _addrs = new ConcurrentDictionary<PeerId, Dictionary<string, ExpiringAddress>>();
            _addrSubs = new ConcurrentDictionary<PeerId, AddrSub[]>();
        }

        public Multiaddress[] Addresses(PeerId p)
        {
            Dictionary<string, ExpiringAddress> maddrs;
            if (!_addrs.TryGetValue(p, out maddrs))
                return Array.Empty<Multiaddress>();

            var now = DateTime.Now;
            var good = new List<Multiaddress>();
            var expired = new List<string>();

            foreach (var addr in maddrs)
            {
                if (addr.Value.ExpiredBy(now))
                    expired.Add(addr.Key);
                else
                    good.Add(addr.Value.Address);
            }

            expired.ForEach(e => maddrs.Remove(e));

            return good.ToArray();
        }

        public void AddAddress(PeerId p, Multiaddress addr, TimeSpan ttl) => AddAddresses(p, new[] {addr}, ttl);

        public void AddAddresses(PeerId p, IEnumerable<Multiaddress> addrs, TimeSpan ttl)
        {
            if (ttl < TimeSpan.Zero)
                return;

            var amap = _addrs.GetOrAdd(p, _ => new Dictionary<string, ExpiringAddress>());
            var exp = DateTime.Now.Add(ttl);
            foreach (var addr in addrs)
            {
                if (addr == null)
                    continue;

                var addrstr = Encoding.UTF8.GetString(addr.ToBytes());
                ExpiringAddress a;
                if (!amap.TryGetValue(addrstr, out a) || exp > a.TTL)
                {
                    amap[addrstr] = new ExpiringAddress(addr, exp);
                }
            }
        }

        public void SetAddress(PeerId p, Multiaddress addr, TimeSpan ttl) => SetAddresses(p, new[] {addr}, ttl);

        public void SetAddresses(PeerId p, IEnumerable<Multiaddress> addrs, TimeSpan ttl)
        {
            var amap = _addrs.GetOrAdd(p, _ => new Dictionary<string, ExpiringAddress>());
            var exp = DateTime.Now.Add(ttl);
            foreach (var addr in addrs)
            {
                if (addr == null)
                    continue;

                var addrstr = Encoding.UTF8.GetString(addr.ToBytes());

                if (ttl > TimeSpan.Zero)
                {
                    amap[addrstr] = new ExpiringAddress(addr, exp);
                }
                else
                {
                    amap.Remove(addrstr);
                }
            }
        }

        public void ClearAddresses(PeerId peer)
        {
            Dictionary<string, ExpiringAddress> a;
            if (_addrs.TryGetValue(peer, out a))
            {
                a.Clear();
            }
        }

        private void RemoveSub(PeerId peer, AddrSub s)
        {
            AddrSub[] subs;
            if (_addrSubs.TryGetValue(peer, out subs))
            {
                var filtered = subs.Where(x => !ReferenceEquals(x, s)).ToArray();
                _addrSubs.TryUpdate(peer, filtered, subs);
            }
        }

        private struct AddrSub
        {
            public BlockingCollection<Multiaddress> pubch;
            public ReaderWriterLockSlim lk;
            public List<Multiaddress> buffer;
            public CancellationToken cts;

            public void PubAddr(Multiaddress a)
            {
                pubch.Add(a, cts);
            }
        }

        /*public BlockingCollection<Multiaddress> AddressStream(PeerId peer, CancellationToken cancellationToken)
        {
            var sub = new AddrSub()
            {
                pubch = new BlockingCollection<Multiaddress>(),
                cts = cancellationToken
            };

            var output = new BlockingCollection<Multiaddress>();
            _addrSubs.AddOrUpdate(peer, p => Array.Empty<AddrSub>(), (p, addrs) => addrs.Concat(new[] {sub}).ToArray());
            Dictionary<string, ExpiringAddress> baseaddrset;
            var initial = _addrs.TryGetValue(peer, out baseaddrset)
                ? baseaddrset.Select(a => a.Value.Address).ToList()
                : new List<Multiaddress>();

            initial.Sort(new AddressListComparer());

            Task.Factory.StartNew(() =>
            {
                var buffer = initial.ToArray().ToList();
                try
                {
                    var sent = buffer.Select(a => a.ToString()).ToList();
                    BlockingCollection<Multiaddress> outch = null;
                    Multiaddress next = null;
                    if (buffer.Count > 0)
                    {
                        next = buffer[0];
                        buffer.Remove(next);
                        outch = output;
                    }

                    var done = false;
                    while (!done)
                    {
                        if (outch?.TryAdd(next, 1, cancellationToken) ?? false)
                        {
                            if (buffer.Count > 0)
                            {
                                next = buffer[0];
                                buffer.Remove(next);
                            }
                            else
                            {
                                outch?.CompleteAdding();
                                outch = null;
                                next = null;
                            }
                            continue;
                        }

                        Multiaddress naddr;
                        if (sub.pubch.TryTake(out naddr, 1, cancellationToken))
                        {
                            if (!sent.Contains(naddr.ToString()))
                            {
                                sent.Add(naddr.ToString());
                                if (next == null)
                                {
                                    next = naddr;
                                    outch = output;
                                }
                                else
                                {
                                    buffer.Add(naddr);
                                }
                            }
                        }
                        else
                        {
                            RemoveSub(peer, sub);
                            done = true;
                        }
                    }

                }
                finally
                {
                    output.CompleteAdding();
                }
            }, cancellationToken);

            return output;
        }*/
    }

    public class AddressListComparer : IComparer<Multiaddress>
    {
        public int Compare(Multiaddress x, Multiaddress y)
        {
            var lbx = x.IsIPLoopback();
            var lby = y.IsIPLoopback();
            if (lbx)
            {
                if (!lby)
                    return -1;
            }

            var fdx = x.IsFDCostlyTransport();
            var fdy = y.IsFDCostlyTransport();
            if (!fdx)
            {
                return fdy ? -1 : 1;
            }

            if (!fdy)
                return 1;

            if (lby)
                return 1;

            return x.ToBytes().Compare(y.ToBytes());
        }
    }
}