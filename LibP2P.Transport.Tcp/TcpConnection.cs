using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Abstractions.Connection;
using LibP2P.Abstractions.Transport;
using Multiformats.Address;

namespace LibP2P.Transport.Tcp
{
    internal class TcpConnection : ITransportConnection
    {
        private readonly IConnection _connection;

        public ITransport Transport { get; }
        public EndPoint LocalAddress => _connection.LocalAddress;
        public Multiaddress LocalMultiaddress => _connection.LocalMultiaddress;
        public EndPoint RemoteAddress => _connection.RemoteAddress;
        public Multiaddress RemoteMultiaddress => _connection.RemoteMultiaddress;

        public TcpConnection(IConnection connection, ITransport transport)
        {
            _connection = connection;
            Transport = transport;
        }

        public void Dispose() => _connection?.Dispose();
        public void SetDeadline(DateTime t) => _connection.SetDeadline(t);
        public void SetReadDeadline(DateTime t) => _connection.SetReadDeadline(t);
        public void SetWriteDeadline(DateTime t) => _connection.SetWriteDeadline(t);
        public int Read(byte[] buffer, int offset, int count) => _connection.Read(buffer, offset, count);
        public int Write(byte[] buffer, int offset, int count) => _connection.Write(buffer, offset, count);
        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _connection.ReadAsync(buffer, offset, count, cancellationToken);
        public Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _connection.WriteAsync(buffer, offset, count, cancellationToken);
    }
}