using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.IO;

namespace LibP2P.Utilities.Extensions
{
    public static class StreamExtensions
    {
        public static IReader AsReader(this Stream stream) => new StreamReader(stream);

        private class StreamReader : IReader
        {
            private readonly Stream _stream;

            public StreamReader(Stream stream)
            {
                _stream = stream;
            }

            public int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public static IWriter AsWriter(this Stream stream) => new StreamWriter(stream);

        private class StreamWriter : IWriter
        {
            private readonly Stream _stream;

            public StreamWriter(Stream stream)
            {
                _stream = stream;
            }

            public int Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
                return count;
            }

            public Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _stream.WriteAsync(buffer, offset, count, cancellationToken)
                    .ContinueWith(t => count, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);
            }
        }

        public static IReadWriter AsReadWriter(this Stream stream) => new StreamReadWriter(stream);

        private class StreamReadWriter : IReadWriter
        {
            private readonly Stream _stream;

            public StreamReadWriter(Stream stream)
            {
                _stream = stream;
            }

            public int Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
                return count;
            }

            public Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _stream.WriteAsync(buffer, offset, count, cancellationToken)
                    .ContinueWith(t => count, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);
            }

            public int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public static IReadWriteCloser AsReadWriteCloser(this Stream stream) => new StreamReadWriteCloser(stream);

        private class StreamReadWriteCloser : IReadWriteCloser
        {
            private readonly Stream _stream;

            public StreamReadWriteCloser(Stream stream)
            {
                _stream = stream;
            }

            public int Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
                return count;
            }

            public Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _stream.WriteAsync(buffer, offset, count, cancellationToken)
                    .ContinueWith(t => count, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled);
            }

            public int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.ReadAsync(buffer, offset, count, cancellationToken);

            public void Close() => _stream.Dispose();
        }

        public static Stream AsSystemStream(this IReader reader) => new WriterStream(reader);
        public static Stream AsSystemStream(this IWriter writer) => new WriterStream(writer);
        public static Stream AsSystemStream(this ISeeker seeker) => new WriterStream(seeker);
        public static Stream AsSystemStream(this IReadWriter rws) => new WriterStream(rws);
        public static Stream AsSystemStream(this IReadWriteSeeker rws) => new WriterStream(rws);
        public static Stream AsSystemStream(this IReadWriteCloser rws) => new WriterStream(rws);

        private class WriterStream : Stream
        {
            private readonly ISeeker _seeker;
            private readonly IReader _reader;
            private readonly IWriter _writer;
            private readonly ICloser _closer;

            public WriterStream(IWriter writer)
            {
                _writer = writer;
            }

            public WriterStream(IReader reader)
            {
                _reader = reader;
            }

            public WriterStream(IReadWriter rws)
            {
                _writer = rws;
                _reader = rws;
            }

            public WriterStream(ISeeker seeker)
            {
                _seeker = seeker;
            }

            public WriterStream(IReadWriteSeeker rws)
            {
                _writer = rws;
                _reader = rws;
                _seeker = rws;
            }

            public WriterStream(IReadWriteCloser rws)
            {
                _writer = rws;
                _reader = rws;
                _closer = rws;
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin) => _seeker.Seek(offset, origin);

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count) => _reader.Read(buffer, offset, count);
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _reader.ReadAsync(buffer, offset, count, cancellationToken);

            public override void Write(byte[] buffer, int offset, int count) => _writer.Write(buffer, offset, count);
            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _writer.WriteAsync(buffer, offset, count, cancellationToken);

            public override bool CanRead => _reader != null;
            public override bool CanSeek => _seeker != null;
            public override bool CanWrite => _writer != null;
            public override long Length => -1;
            public override long Position { get; set; }

            public override void Close() => _closer?.Close();
        }

        public static int CopyTo(this IReader src, IWriter dst, int bufferSize = 4096)
        {
            var buffer = new byte[bufferSize];
            var bytesRead = 0;
            var result = 0;
            while ((bytesRead = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                var totalWritten = 0;
                while (totalWritten < bytesRead)
                {
                    var bytesWritten = dst.Write(buffer, totalWritten, bytesRead - totalWritten);
                    if (bytesWritten <= 0)
                        return result;

                    totalWritten += bytesWritten;
                }
                result += totalWritten;
            }

            return result;
        }

        public static async Task<int> CopyToAsync(this IReader src, IWriter dst, int bufferSize = 4096, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new byte[bufferSize];
            var bytesRead = 0;
            var result = 0;
            while ((bytesRead = await src.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                var totalWritten = 0;
                while (totalWritten < bytesRead)
                {
                    var bytesWritten = await dst.WriteAsync(buffer, totalWritten, bytesRead - totalWritten, cancellationToken);
                    if (bytesWritten <= 0)
                        return result;

                    totalWritten += bytesWritten;
                }
                result += totalWritten;
            }

            return result;
        }

        public static int ReadFull(this IReader src, byte[] buffer, int offset = 0, int count = -1)
        {
            if (count == -1)
                count = buffer.Length - offset;

            var read = 0;
            while (read < count)
            {
                var n = src.Read(buffer, read, count - read);
                if (n <= 0)
                    return read;

                read += n;
            }
            return read;
        }

        public static async Task<int> ReadFullAsync(this IReader src, byte[] buffer, int offset = 0, int count = -1, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (count == -1)
                count = buffer.Length - offset;

            var read = 0;
            while (read < count && !cancellationToken.IsCancellationRequested)
            {
                var n = await src.ReadAsync(buffer, read, count - read, cancellationToken);
                if (n <= 0)
                    return read;

                read += n;
            }
            return read;
        }
    }
}
