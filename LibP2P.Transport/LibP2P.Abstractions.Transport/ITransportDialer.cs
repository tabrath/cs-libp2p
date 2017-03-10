using System.Threading;
using System.Threading.Tasks;
using LibP2P.Abstractions.Connection;
using Multiformats.Address;

namespace LibP2P.Abstractions.Transport
{
    public interface ITransportDialer
    {
        ITransportConnection Dial(Multiaddress raddr);
        Task<ITransportConnection> DialAsync(Multiaddress raddr, CancellationToken cancellationToken);
        bool Matches(Multiaddress ma);
    }
}