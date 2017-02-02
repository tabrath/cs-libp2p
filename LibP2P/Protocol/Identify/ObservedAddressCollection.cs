using System;
using System.Collections.Generic;
using System.Linq;
using LibP2P.Peer.Store;
using LibP2P.Utilities;
using Multiformats.Address;

namespace LibP2P.Protocol.Identify
{
    public class ObservedAddressCollection : SyncMutex
    {
        private Dictionary<string, ObservedAddress> _addresses;
        private TimeSpan _ttl;

        public Multiaddress[] Addresses
        {
            get
            {
                return Write(() =>
                {
                    if (_addresses == null)
                        return Array.Empty<Multiaddress>();

                    var now = DateTime.Now;
                    var addrs = new List<Multiaddress>();

                    foreach (var oa in _addresses)
                    {
                        if (now.Subtract(oa.Value.LastSeen ?? now) > _ttl)
                        {
                            _addresses.Remove(oa.Key);
                            continue;
                        }

                        if (oa.Value.SeenBy?.Count > 1)
                        {
                            addrs.Add(oa.Value.Address);
                        }
                    }

                    return addrs.ToArray();
                });
            }
        }

        public TimeSpan TTL
        {
            get
            {
                return Write(() =>
                {
                    if (_addresses == null)
                        _ttl = AddressManager.OwnObservedAddrTTL;

                    return _ttl;
                });
            }
            set { Write(() => _ttl = value); }
        }

        public ObservedAddressCollection()
            : base()
        {
        }

        public void Add(Multiaddress address, Multiaddress observer)
        {
            Write(() =>
            {
                if (_addresses == null)
                {
                    _addresses = new Dictionary<string, ObservedAddress>();
                    _ttl = AddressManager.OwnObservedAddrTTL;
                }

                var s = address.ToString();
                ObservedAddress oa;
                if (!_addresses.TryGetValue(s, out oa))
                {
                    oa = new ObservedAddress(address, new HashSet<string>());
                    _addresses.Add(s, oa);
                }

                oa.SeenBy.Add(ToObserverGroup(observer));
                oa.LastSeen = DateTime.Now;
            });
        }

        private static string ToObserverGroup(Multiaddress address) => address.Split().First().ToString();
    }
}