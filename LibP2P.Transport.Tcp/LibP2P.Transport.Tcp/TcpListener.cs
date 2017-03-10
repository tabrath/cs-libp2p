using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Abstractions.Connection;
using LibP2P.Abstractions.Transport;
using Multiformats.Address;

namespace LibP2P.Transport.Tcp
{
    internal class TcpListener : ITransportListener
    {
        private readonly TcpTransport _transport;
        private readonly IListener _listener;

        public EndPoint Address => _listener.Address;
        public Multiaddress Multiaddress => _listener.Multiaddress;

        public TcpListener(TcpTransport transport, Multiaddress laddr, bool reusePort)
        {
            _transport = transport;
            _listener = new SocketListener(laddr, reusePort);
        }

        public ITransportConnection Accept()
        {
            var conn = _listener.Accept();

            return conn != null ? new TcpConnection(conn, _transport) : null;
        }

        public Task<ITransportConnection> AcceptAsync(CancellationToken cancellationToken)
        {
            return _listener.AcceptAsync(cancellationToken)
                .ContinueWith(t => new TcpConnection(t.Result, _transport) as ITransportConnection, cancellationToken, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
        }

        public void SetAddressFilters(ICollection<EndPoint> filters) => _listener.SetAddressFilters(filters);

        public void Dispose() => _listener.Dispose();
    }
}