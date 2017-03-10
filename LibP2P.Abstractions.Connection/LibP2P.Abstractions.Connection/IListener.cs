using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Peer;
using Multiformats.Address;

namespace LibP2P.Abstractions.Connection
{
    public interface IListener : IDisposable
    {
        EndPoint Address { get; }
        Multiaddress Multiaddress { get; }
        PeerId LocalPeer { get; }

        IConnection Accept();
        Task<IConnection> AcceptAsync(CancellationToken cancellationToken);
        void SetAddressFilters(ICollection<EndPoint> filters);
    }
}