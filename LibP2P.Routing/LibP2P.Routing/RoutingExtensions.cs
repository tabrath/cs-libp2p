using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.Crypto;
using LibP2P.Peer;

namespace LibP2P.Routing
{
    public static class RoutingExtensions
    {
        public static string KeyForPublicKey(this PeerId peer) => $"/pk/{peer}";

        public static async Task<PublicKey> GetPublicKey(this IValueStore r, byte[] pkhash,
            CancellationToken cancellationToken)
        {
            var dht = r as IPublicKeyFetcher;
            if (dht != null)
                return await dht.GetPublicKey(new PeerId(pkhash), cancellationToken);

            var key = $"/pk/{Encoding.UTF8.GetString(pkhash)}";
            var pkval = await r.GetValue(key, cancellationToken);

            return pkval != null ? PublicKey.Unmarshal(pkval) : null;
        }
    }
}