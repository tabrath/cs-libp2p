using System;
using System.Net;
using LibP2P.IO;
using Multiformats.Address;

namespace LibP2P.Abstractions.Connection
{
    public interface IConnection : IReader, IWriter, IDisposable
    {
        EndPoint LocalAddress { get; }
        Multiaddress LocalMultiaddress { get; }
        EndPoint RemoteAddress { get; }
        Multiaddress RemoteMultiaddress { get; }

        void SetDeadline(DateTime t);
        void SetReadDeadline(DateTime t);
        void SetWriteDeadline(DateTime t);
    }
}