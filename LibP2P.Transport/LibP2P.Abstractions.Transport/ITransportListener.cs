using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Abstractions.Connection;
using Multiformats.Address;

namespace LibP2P.Abstractions.Transport
{
    public interface ITransportListener : IDisposable
    {
        EndPoint Address { get; }
        Multiaddress Multiaddress { get; }

        ITransportConnection Accept();
        Task<ITransportConnection> AcceptAsync(CancellationToken cancellationToken);
    }
}