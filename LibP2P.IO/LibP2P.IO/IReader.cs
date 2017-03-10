using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.IO
{
    public interface IReader
    {
        int Read(byte[] buffer, int offset, int count);
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}