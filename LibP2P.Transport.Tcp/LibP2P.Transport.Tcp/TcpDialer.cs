using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Abstractions.Connection;
using LibP2P.Abstractions.Transport;
using Multiformats.Address;
using Multiformats.Address.Net;
using Multiformats.Address.Protocols;

namespace LibP2P.Transport.Tcp
{
    internal class TcpDialer : ITransportDialer
    {
        private readonly TcpTransport _transport;
        private readonly IDialer _dialer;

        public EndPoint LocalAddress => _dialer.LocalAddress;
        public Multiaddress LocalMultiaddress => _dialer.LocalMultiaddress;

        public TcpDialer(TcpTransport t, Multiaddress laddr, TimeSpan? timeout, bool reusePort)
        {
            var la = laddr.ToEndPoint();
            if (la == null)
                throw new Exception("Invalid address");

            _transport = t;
            _dialer = new SocketDialer(laddr, timeout, reusePort);
        }

        public bool Matches(Multiaddress ma) => ma.Protocols.OfType<TCP>().Any();

        public ITransportConnection Dial(Multiaddress raddr)
        {
            var conn = _dialer.Dial(raddr);

            return conn != null ? new TcpConnection(conn, _transport) : null;
        }

        public Task<ITransportConnection> DialAsync(Multiaddress remoteAddress, CancellationToken cancellationToken)
        {
            return _dialer.DialAsync(remoteAddress, cancellationToken)
                .ContinueWith(t => new TcpConnection(t.Result, _transport) as ITransportConnection, cancellationToken, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
        }
    }
}