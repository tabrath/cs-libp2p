using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Multiformats.Address;
using Multiformats.Address.Net;

namespace LibP2P.Abstractions.Connection
{
    public class SocketConnection : IConnection
    {
        private readonly Socket _socket;

        public EndPoint LocalAddress { get; }
        public Multiaddress LocalMultiaddress { get; }
        public EndPoint RemoteAddress { get; }
        public Multiaddress RemoteMultiaddress { get; }
        public DateTime Deadline { get; protected set; }

        public SocketConnection(Socket socket)
        {
            _socket = socket;
            LocalAddress = _socket.LocalEndPoint;
            LocalMultiaddress = _socket.GetLocalMultiaddress();
            RemoteAddress = _socket.RemoteEndPoint;
            RemoteMultiaddress = _socket.GetRemoteMultiaddress();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return _socket.Receive(buffer, offset, count, SocketFlags.None);
            }
            catch
            {
                return -1;
            }
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.Factory.FromAsync(
                (buf, flags, cb, state) => ((Socket)state).BeginReceive(buf.Array, buf.Offset, buf.Count, flags, cb, state),
                asyncResult => ((Socket)asyncResult.AsyncState).EndReceive(asyncResult),
                new ArraySegment<byte>(buffer, offset, count), SocketFlags.None, _socket);
        }

        public int Write(byte[] buffer, int offset, int count)
        {
            try
            {
                return _socket.Send(buffer, offset, count, SocketFlags.None);
            }
            catch
            {
                return -1;
            }
        }

        public Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.Factory.FromAsync(
                (buf, flags, cb, state) => ((Socket)state).BeginSend(buf.Array, buf.Offset, buf.Count, flags, cb, state),
                asyncResult => ((Socket)asyncResult.AsyncState).EndSend(asyncResult),
                new ArraySegment<byte>(buffer, offset, count), SocketFlags.None, _socket);
        }


        public void SetDeadline(DateTime t)
        {
            Deadline = t;
        }

        public void SetReadDeadline(DateTime t) => _socket.ReceiveTimeout = (int) DateTime.Now.Subtract(t).TotalMilliseconds;
        public void SetWriteDeadline(DateTime t) => _socket.SendTimeout = (int) DateTime.Now.Subtract(t).TotalMilliseconds;
        public void Dispose() => _socket.Dispose();

    }
}