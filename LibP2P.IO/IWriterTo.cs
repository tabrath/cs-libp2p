using System.Threading;
using System.Threading.Tasks;

namespace LibP2P.IO
{
    public interface IWriterTo
    {
        long WriteTo(IWriter writer);
        Task<long> WriteToAsync(IWriter writer, CancellationToken cancellationToken);
    }
}