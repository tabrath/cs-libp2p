using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Peer;
using LibP2P.Utilities;
using Multiformats.Address;
using Multiformats.Address.Net;

namespace LibP2P.Abstractions.Connection
{
    public class SocketListener : IListener
    {
        public static SocketListener Default() => new SocketListener(null, true);

        private readonly bool _reusePort;
        private readonly Socket _socket;
        private ICollection<EndPoint> _filters;

        public EndPoint Address { get; }
        public Multiaddress Multiaddress { get; }
        public PeerId LocalPeer { get; }

        public SocketListener(Multiaddress localAddress, bool reusePort)
        {
            _reusePort = reusePort;
            SocketType st;
            ProtocolType pt;

            EndPoint addr;
            if (localAddress != null)
                addr = localAddress.ToEndPoint(out pt, out st);
            else
            {
                st = SocketType.Stream;
                pt = ProtocolType.Tcp;
                addr = new IPEndPoint(IPAddress.Loopback, 0);
            }

            _socket = new Socket(addr.AddressFamily, st, pt);
            if (reusePort)
            {
                _socket.ExclusiveAddressUse = false;
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            _socket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            _socket.Bind(addr);
            _socket.Listen(512);

            Address = _socket.LocalEndPoint;
            Multiaddress = _socket.GetLocalMultiaddress();
        }

        public void Dispose() => _socket.Dispose();

        public IConnection Accept()
        {
            try
            {
                var conn = _socket.Accept();

                if (_filters?.Contains(conn.RemoteEndPoint) ?? false)
                {
                    conn.Dispose();
                    return null;
                }

                SetupConnection(conn);

                return new SocketConnection(conn);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void SetupConnection(Socket conn)
        {
            conn?.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
        }

        public Task<IConnection> AcceptAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<IConnection>();

            _socket.BeginAccept(ar =>
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var conn = ((Socket) ar.AsyncState).EndAccept(ar);

                    if (_filters?.Contains(conn.RemoteEndPoint) ?? false)
                    {
                        conn.Dispose();
                        tcs.TrySetResult(null);
                    }

                    SetupConnection(conn);

                    tcs.TrySetResult(new SocketConnection(conn));
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            }, _socket);

            return tcs.Task;
        }

        public void SetAddressFilters(ICollection<EndPoint> filters)
        {
            _filters = filters;
        }
    }
}
