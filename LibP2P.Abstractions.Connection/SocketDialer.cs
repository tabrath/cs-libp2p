using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Multiformats.Address;
using Multiformats.Address.Net;

namespace LibP2P.Abstractions.Connection
{
    public class SocketDialer : IDialer
    {
        private static readonly Lazy<SocketDialer> _default = new Lazy<SocketDialer>(() => new SocketDialer(null, null, true));
        public static SocketDialer Default => _default.Value;

        private readonly TimeSpan? _timeout;
        private readonly bool _reusePort;
        public EndPoint LocalAddress { get; }
        public Multiaddress LocalMultiaddress { get; }

        public SocketDialer(Multiaddress localAddress, TimeSpan? timeout, bool reusePort)
        {
            _timeout = timeout;
            _reusePort = reusePort;
            LocalAddress = localAddress?.ToEndPoint();
            LocalMultiaddress = localAddress;
        }

        public IConnection Dial(Multiaddress remoteAddress)
        {
            ProtocolType p;
            SocketType s;
            var ip = remoteAddress.ToEndPoint(out p, out s);

            return Dial(ip, s, p);
        }

        public IConnection Dial(EndPoint remoteAddress, SocketType socketType, ProtocolType protocolType)
        {
            var socket = SetupConnection(remoteAddress, socketType, protocolType);

            socket.Connect(remoteAddress);

            return new SocketConnection(socket);
        }

        public Task<IConnection> DialAsync(Multiaddress remoteAddress, CancellationToken cancellationToken)
        {
            ProtocolType p;
            SocketType s;
            var ip = remoteAddress.ToEndPoint(out p, out s);

            return DialAsync(ip, s, p, cancellationToken);
        }

        public Task<IConnection> DialAsync(EndPoint remoteAddress, SocketType socketType,
            ProtocolType protocolType, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IConnection>();

            var socket = SetupConnection(remoteAddress, socketType, protocolType);

            socket.BeginConnect(remoteAddress, ar =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.TrySetCanceled();
                    return;
                }
                try
                {
                    var s = (Socket)ar.AsyncState;
                    s.EndConnect(ar);
                    tcs.TrySetResult(new SocketConnection(s));
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }

            }, socket);

            return tcs.Task;
        }

        private Socket SetupConnection(EndPoint remoteAddress, SocketType socketType, ProtocolType protocolType)
        {
            var socket = new Socket(remoteAddress.AddressFamily, socketType, protocolType);

            if (_reusePort)
            {
                socket.ExclusiveAddressUse = false;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);

            if (LocalAddress != null)
                socket.Bind(LocalAddress);

            return socket;
        }
    }
}
