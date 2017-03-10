using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.IO
{
    public interface IWriter
    {
        int Write(byte[] buffer, int offset, int count);
        Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}