using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.Routing
{
    public interface IValueStore
    {
        Task PutValue(string key, byte[] value, CancellationToken cancellationToken);
        Task<byte[]> GetValue(string key, CancellationToken cancellationToken);
        Task<ReceivedValue[]> GetValues(string key, int count, CancellationToken cancellationToken);
    }
}