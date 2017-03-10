using System;
using System.Collections.Concurrent;
using System.Linq;
using Multiformats.Address;
using Multiformats.Address.Protocols;

namespace LibP2P.Abstractions.Transport
{
    public abstract class BaseTransport<TProtocol> : ITransport
        where TProtocol : MultiaddressProtocol
    {
        private readonly ConcurrentDictionary<string, ITransportDialer> _dialers;
        private readonly ConcurrentDictionary<string, ITransportListener> _listeners;

        protected BaseTransport()
        {
            _dialers = new ConcurrentDictionary<string, ITransportDialer>();
            _listeners = new ConcurrentDictionary<string, ITransportListener>();
        }

        public ITransportDialer Dialer(Multiaddress laddr, TimeSpan? timeout = null, bool reusePort = true)
        {
            ITransportDialer d;
            if (_dialers.TryGetValue(laddr.ToString(), out d))
                return d;

            d = CreateDialer(laddr, timeout, reusePort);
            _dialers.TryAdd(laddr.ToString(), d);
            return d;
        }

        protected abstract ITransportDialer CreateDialer(Multiaddress laddr, TimeSpan? timeout = null, bool reusePort = true);

        public ITransportListener Listen(Multiaddress laddr)
        {
            if (!Matches(laddr))
                throw new NotSupportedException($"Tcp transport cannot listen on {laddr}");

            ITransportListener l;
            if (_listeners.TryGetValue(laddr.ToString(), out l))
                return l;

            l = CreateListener(laddr);
            _listeners.TryAdd(laddr.ToString(), l);
            return l;
        }

        protected abstract ITransportListener CreateListener(Multiaddress laddr, bool reusePort = true);

        public bool Matches(Multiaddress ma) => ma.Protocols.OfType<TProtocol>().Any();
    }
}