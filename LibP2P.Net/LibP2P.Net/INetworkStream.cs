using System;
using LibP2P.IO;
using LibP2P.Protocol;

namespace LibP2P.Net
{
    public interface INetworkStream : IReader, IWriter, IDisposable
    {
        ProtocolId Protocol { get; set; }
        INetworkConnection Connection { get; }

        void SetDeadline(DateTime t);
        void SetReadDeadline(TimeSpan t);
        void SetWriteDeadline(TimeSpan t);
    }

    public delegate void StreamHandler(INetworkStream stream);
}