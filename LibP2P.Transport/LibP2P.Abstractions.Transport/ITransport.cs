using Multiformats.Address;
using System;

namespace LibP2P.Abstractions.Transport
{
    public interface ITransport
    {
        ITransportDialer Dialer(Multiaddress localAddress, TimeSpan? timeout = null, bool reusePort = true);
        ITransportListener Listen(Multiaddress localAddress);
        bool Matches(Multiaddress ma);
    }
}
