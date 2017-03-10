using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Multiformats.Address;

namespace LibP2P.Abstractions.Connection
{
    public interface IDialer
    {
        EndPoint LocalAddress { get; }
        Multiaddress LocalMultiaddress { get; }

        IConnection Dial(Multiaddress address);
        Task<IConnection> DialAsync(Multiaddress address, CancellationToken cancellationToken);
    }
}
