using System;
using LibP2P.Abstractions.Transport;
using Multiformats.Address;
using Multiformats.Address.Protocols;

namespace LibP2P.Transport.Tcp
{
    public class TcpTransport : BaseTransport<TCP>
    {
        public TcpTransport()
            : base()
        {
        }

        protected override ITransportDialer CreateDialer(Multiaddress laddr, TimeSpan? timeout = null, bool reusePort = true) => new TcpDialer(this, laddr, timeout, reusePort);
        protected override ITransportListener CreateListener(Multiaddress laddr, bool reusePort = true) => new TcpListener(this, laddr, reusePort);
    }
}
