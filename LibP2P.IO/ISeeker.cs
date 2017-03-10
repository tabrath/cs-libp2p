using System.IO;

namespace LibP2P.IO
{
    public interface ISeeker
    {
        long Seek(long offset, SeekOrigin whence);
    }
}