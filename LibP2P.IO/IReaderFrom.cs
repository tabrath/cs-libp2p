using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.IO
{
    public interface IReaderFrom
    {
        long ReadFrom(IReader reader);
        Task<long> ReadFromAsync(IReader reader, CancellationToken cancellationToken);
    }
}