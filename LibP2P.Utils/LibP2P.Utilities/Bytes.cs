using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.IO;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Utilities
{
    public static class Bytes
    {
        public class Reader : IReader, /*IReaderAt,*/ IWriterTo, ISeeker, IByteScanner
        {
            private byte[] _bytes;
            private int _index;

            public int Length => _bytes.Length - _index;
            public int Size => _bytes.Length;

            public Reader(byte[] bytes)
            {
                _bytes = bytes;
                _index = 0;
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                if (_index >= _bytes.Length)
                    return 0;

                var n = _bytes.Slice(_index).Copy(buffer, offset);
                _index += n;
                return n;
            }

            public int ReadAt(byte[] buffer, int bufferOffset, long offset)
            {
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                if (offset >= _bytes.Length)
                    return 0;

                var n = _bytes.Slice((int) offset).Copy(buffer, bufferOffset);
                if (n < buffer.Length - bufferOffset)
                    throw new EndOfStreamException();

                return n;
            }

            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return Task.FromResult(Read(buffer, offset, count));
            }

            public long WriteTo(IWriter writer)
            {
                if (_index >= _bytes.Length)
                    return 0;

                var b = _bytes.Slice(_index);
                var m = writer.Write(b, 0, b.Length);
                if (m > b.Length)
                    throw new Exception("Invalid write count");

                _index += m;

                return m;
            }

            public async Task<long> WriteToAsync(IWriter writer, CancellationToken cancellationToken)
            {
                if (_index >= _bytes.Length)
                    return 0;

                var b = _bytes.Slice(_index);
                var m = await writer.WriteAsync(b, 0, b.Length, cancellationToken);
                if (m > b.Length)
                    throw new Exception("Invalid write count");

                _index += m;

                return m;
            }

            public long Seek(long offset, SeekOrigin whence)
            {
                long abs = 0;
                switch (whence)
                {
                    case SeekOrigin.Begin:
                        abs = offset;
                        break;
                    case SeekOrigin.Current:
                        abs = _index + offset;
                        break;
                    case SeekOrigin.End:
                        abs = _bytes.Length + offset;
                        break;
                }

                if (abs < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                _index = (int)abs;
                return abs;
            }

            public byte ReadByte()
            {
                if (_index >= _bytes.Length)
                    return 0;

                return _bytes[_index++];
            }

            public void UnreadByte()
            {
                if (_index <= 0)
                    throw new Exception("At beginning");

                _index--;
            }

            public void Reset(byte[] b)
            {
                _bytes = b;
                _index = 0;
            }
        }
    }
}
