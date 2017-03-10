using System.Linq;
using System.Net;
using Multiformats.Address;
using Multiformats.Address.Net;
using Multiformats.Address.Protocols;

namespace LibP2P.Utilities.Extensions
{
    public static class MultiaddressExtensions
    {
        public static bool IsIPLoopback(this Multiaddress ma)
        {
            var ep = ma.ToEndPoint();
            if (ep == null)
                return false;

            return ep.Address.Equals(IPAddress.Loopback) || ep.Address.Equals(IPAddress.IPv6Loopback);
        }

        public static bool IsFDCostlyTransport(this Multiaddress ma)
        {
            return ma.Protocols.OfType<TCP>().Any();
        }
    }
}
