using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibP2P.IO;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Utilities
{
    public static class BufferIO
    {
        public const int DefaultBufferSize = 4096;
        private const int MinReadBufferSize = 16;
        private const int MaxConsecutiveEmptyReads = 100;

        public class Reader : IReader, IByteScanner, IWriterTo
        {
            private byte[] _buffer;
            private IReader _rd;
            private int _r;
            private int _w;
            private int _lastByte;

            public int Buffered => _w - _r;

            protected Reader()
            {
            }

            public static IReader New(IReader rd, int size = DefaultBufferSize)
            {
                var b = (Reader)rd;
                if (b != null && b._buffer.Length >= size)
                    return b;

                if (size < MinReadBufferSize)
                    size = MinReadBufferSize;

                var r = new Reader();
                r.Reset(new byte[size], rd);
                return r;
            }

            public int Read(byte[] buffer, int offset, int count)
            {
                if (count == 0)
                    return 0;

                int n;
                if (_r == _w)
                {
                    if (count >= _buffer.Length)
                    {
                        n = _rd.Read(buffer, offset, count);
                        if (n < 0)
                            throw new Exception("Negative read");
                        if (n > 0)
                        {
                            _lastByte = buffer[offset + n - 1];
                        }
                        return n;
                    }
                    Fill();
                    if (_r == _w)
                        return 0;
                }

                n = _buffer.Slice(_r, _buffer.Length - _w).Copy(buffer, offset, count);
                _r += n;
                _lastByte = _buffer[_r - 1];
                return n;
            }

            public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (count == 0)
                    return 0;

                int n;
                if (_r == _w)
                {
                    if (count >= _buffer.Length)
                    {
                        n = await _rd.ReadAsync(buffer, offset, count, cancellationToken);
                        if (n < 0)
                            throw new Exception("Negative read");
                        if (n > 0)
                        {
                            _lastByte = buffer[offset + n - 1];
                        }
                        return n;
                    }
                    Fill();
                    if (_r == _w)
                        return 0;
                }

                n = _buffer.Slice(_r, _buffer.Length - _w).Copy(buffer, offset, count);
                _r += n;
                _lastByte = _buffer[_r - 1];
                return n;
            }

            public byte ReadByte()
            {
                while (_r == _w)
                {
                    Fill();
                }

                var c = _buffer[_r];
                _r++;
                _lastByte = c;
                return c;
            }

            public void UnreadByte()
            {
                if (_lastByte < 0 || _r == 0 && _w > 0)
                    throw new Exception("Invalid unread byte");

                if (_r > 0)
                    _r--;
                else
                    _w = 1;

                _buffer[_r] = (byte)_lastByte;
                _lastByte = -1;
            }

            public void Reset(IReader r) => Reset(_buffer, r);

            private void Reset(byte[] buffer, IReader rd)
            {
                _buffer = buffer;
                _rd = rd;
                _lastByte = -1;
            }

            private void Fill()
            {
                if (_r > 0)
                {
                    _buffer.Slice(_r, _r - _w).Copy(_buffer);
                    _w -= _r;
                    _r = 0;
                }

                if (_w >= _buffer.Length)
                    throw new Exception("Tried to fill full buffer");

                for (var i = MaxConsecutiveEmptyReads; i > 0; i--)
                {
                    var n = _rd.Read(_buffer, _w, _buffer.Length - _w);
                    if (n < 0)
                        throw new Exception("Negative read");

                    _w += n;

                    if (n > 0)
                        return;
                }
            }

            public byte[] Peek(int n)
            {
                if (n < 0)
                    throw new ArgumentOutOfRangeException(nameof(n));

                while (_w - _r < n && _w - _r < _buffer.Length)
                {
                    Fill();
                }

                if (n > _buffer.Length)
                    return _buffer.Slice(_r, _buffer.Length - _w);

                var avail = _w - _r;
                if (avail < n)
                    n = avail;

                return _buffer.Slice(_r, _r + n);
            }

            public int Discard(int n)
            {
                if (n < 0)
                    throw new ArgumentOutOfRangeException(nameof(n));

                if (n == 0)
                    return 0;

                var remain = n;

                while (true)
                {
                    var skip = Buffered;
                    if (skip == 0)
                    {
                        Fill();
                        skip = Buffered;
                    }
                    if (skip > remain)
                        skip = remain;
                    _r += skip;
                    remain -= skip;
                    if (remain == 0)
                        return n;
                }
            }

            public long WriteTo(IWriter writer)
            {
                var n = WriteBuffer(writer);

                var r = (IWriterTo) _rd;
                if (r != null)
                    return n + r.WriteTo(writer);

                var w = (IReaderFrom) writer;
                if (w != null)
                    return n + w.ReadFrom(_rd);

                if (_w - _r < _buffer.Length)
                    Fill();

                while (_r < _w)
                {
                    var m = WriteBuffer(writer);
                    n += m;
                    Fill();
                }

                return n;
            }

            public async Task<long> WriteToAsync(IWriter writer, CancellationToken cancellationToken)
            {
                var n = await WriteBufferAsync(writer, cancellationToken);

                var r = (IWriterTo)_rd;
                if (r != null)
                    return n + await r.WriteToAsync(writer, cancellationToken);

                var w = (IReaderFrom)writer;
                if (w != null)
                    return n + await w.ReadFromAsync(_rd, cancellationToken);

                if (_w - _r < _buffer.Length)
                    Fill();

                while (_r < _w)
                {
                    var m = await WriteBufferAsync(writer, cancellationToken);
                    n += m;
                    Fill();
                }

                return n;
            }

            private long WriteBuffer(IWriter w)
            {
                var n = w.Write(_buffer, _r, _buffer.Length - _w);
                if (n < 0)
                    throw new Exception("Negative write");

                _r += n;
                return n;
            }

            private async Task<long> WriteBufferAsync(IWriter w, CancellationToken cancellationToken)
            {
                var n = await w.WriteAsync(_buffer, _r, _buffer.Length - _w, cancellationToken);
                if (n < 0)
                    throw new Exception("Negative write");

                _r += n;
                return n;
            }

            public byte[] ReadSlice(byte delim)
            {
                var slice = Array.Empty<byte>();

                while (true)
                {
                    var i = _buffer.Slice(_r, _buffer.Length - _w).IndexOf(delim);
                    if (i >= 0)
                    {
                        slice = _buffer.Slice(_r, _buffer.Length - (_r + i + 1));
                        _r += i + 1;
                        break;
                    }

                    if (Buffered >= _buffer.Length)
                    {
                        _r = _w;
                        slice = _buffer;
                        break;
                    }

                    Fill();
                }

                if (slice.Length > 0)
                {
                    _lastByte = slice[slice.Length - 1];
                }

                return slice;
            }

            public byte[] ReadLine(out bool isPrefix)
            {
                isPrefix = false;
                byte[] line = Array.Empty<byte>();
                try
                {
                    line = ReadSlice((byte) '\n');
                }
                catch
                {
                    if (line.Length > 0 && line[line.Length - 1] == (byte) '\r')
                    {
                        if (_r == 0)
                            throw new Exception("Tried to rewind past start");
                        _r--;
                        line = line.Slice(0, line.Length - 1);
                    }

                    isPrefix = true;
                    return line;
                }

                if (line.Length == 0)
                    return null;

                if (line[line.Length - 1] == (byte) '\n')
                {
                    var drop = line.Length > 1 && line[line.Length - 2] == (byte) '\r' ? 2 : 1;

                    line = line.Slice(0, line.Length - drop);
                }

                return line;
            }

            public byte[] ReadBytes(byte delim)
            {
                var frag = Array.Empty<byte>();
                var full = new List<byte[]>();
                byte[] buf;

                while (true)
                {
                    try
                    {
                        frag = ReadSlice(delim);
                        break;
                    }
                    catch
                    {
                        buf = new byte[frag.Length];
                        frag.Copy(buf);
                        full.Add(buf);
                    }
                }

                var n = full.Sum(i => i.Length) + frag.Length;
                buf = new byte[n];
                n = full.Aggregate(0, (current, t) => current + t.Copy(buf, current));
                frag.Copy(buf, n);
                return buf;
            }

            public string ReadString(byte delim) => Encoding.UTF8.GetString(ReadBytes(delim));
        }

        public class Writer : IWriter, IByteWriter, IReaderFrom
        {
            private readonly byte[] _buffer;
            private IWriter _wr;
            private int _n;

            public int Available => _buffer.Length - _n;
            public int Buffered => _n;

            protected Writer(byte[] buffer, IWriter writer)
            {
                _buffer = buffer;
                _wr = writer;
            }

            public static Writer New(IWriter w, int size = DefaultBufferSize)
            {
                var b = (Writer) w;
                if (b != null && b._buffer.Length >= size)
                    return b;

                if (size <= 0)
                    size = DefaultBufferSize;

                return new Writer(new byte[size], w);

            }

            public void Reset(IWriter w)
            {
                _n = 0;
                _wr = w;
            }

            public void Flush()
            {
                if (_n == 0)
                    return;

                var n = _wr.Write(_buffer, 0, _n);
                if (n < _n)
                    throw new Exception("Short write");

                _n = 0;
            }

            public async Task FlushAsync(CancellationToken cancellationToken)
            {
                if (_n == 0)
                    return;

                var n = await _wr.WriteAsync(_buffer, 0, _n, cancellationToken);
                if (n < _n)
                    throw new Exception("Short write");

                _n = 0;
            }

            public int Write(byte[] buffer, int offset, int count)
            {
                int nn = 0;
                int n = 0;
                while ((buffer.Length - offset) - count > Available)
                {
                    if (Buffered == 0)
                    {
                        n = _wr.Write(buffer, offset, count);
                    }
                    else
                    {
                        n = buffer.Slice(offset, count).Copy(_buffer, _n);
                        _n += n;
                        Flush();
                    }
                    nn += n;
                    offset += n;
                    count -= n;
                }
                n = buffer.Copy(_buffer, _n);
                _n += n;
                nn += n;
                return nn;
            }

            public async Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                int nn = 0;
                int n = 0;
                while ((buffer.Length - offset) - count > Available)
                {
                    if (Buffered == 0)
                    {
                        n = await _wr.WriteAsync(buffer, offset, count, cancellationToken);
                    }
                    else
                    {
                        n = buffer.Slice(offset, count).Copy(_buffer, _n);
                        _n += n;
                        await FlushAsync(cancellationToken);
                    }
                    nn += n;
                    offset += n;
                    count -= n;
                }
                n = buffer.Copy(_buffer, _n);
                _n += n;
                nn += n;
                return nn;
            }

            //TODO: bug! should return nothing
            public byte WriteByte(byte b)
            {
                if (Available <= 0)
                {
                    Flush();
                }

                _buffer[_n++] = b;
                return 0;
            }

            public int WriteString(string s)
            {
                var n = 0;
                var nn = 0;
                var bytes = Encoding.UTF8.GetBytes(s);
                while (bytes.Length > Available)
                {
                    n = bytes.Copy(_buffer, _n);
                    _n += n;
                    nn += n;
                    bytes = bytes.Slice(n);
                    Flush();
                }

                n = bytes.Copy(_buffer, _n);
                _n += n;
                nn += n;
                return nn;
            }

            public long ReadFrom(IReader reader)
            {
                if (Buffered == 0)
                {
                    var w = (IReaderFrom) _wr;
                    if (w != null)
                        return w.ReadFrom(reader);
                }

                long n = 0;
                var m = 0;
                while (true)
                {
                    if (Available == 0)
                    {
                        Flush();
                        break;
                    }

                    var nr = 0;
                    while (nr < MaxConsecutiveEmptyReads)
                    {
                        m = reader.Read(_buffer, _n, _buffer.Length - _n);
                        if (m != 0)
                            break;

                        nr++;
                    }
                    if (nr == MaxConsecutiveEmptyReads)
                        return n;

                    _n += m;
                }

                return n;
            }

            public async Task<long> ReadFromAsync(IReader reader, CancellationToken cancellationToken)
            {
                if (Buffered == 0)
                {
                    var w = (IReaderFrom)_wr;
                    if (w != null)
                        return await w.ReadFromAsync(reader, cancellationToken);
                }

                long n = 0;
                var m = 0;
                while (true)
                {
                    if (Available == 0)
                    {
                        await FlushAsync(cancellationToken);
                        break;
                    }

                    var nr = 0;
                    while (nr < MaxConsecutiveEmptyReads)
                    {
                        m = await reader.ReadAsync(_buffer, _n, _buffer.Length - _n, cancellationToken);
                        if (m != 0)
                            break;

                        nr++;
                    }
                    if (nr == MaxConsecutiveEmptyReads)
                        return n;

                    _n += m;
                }

                return n;
            }
        }

        public class ReadWriter : IReadWriter
        {
            private readonly Reader _r;
            private readonly Writer _w;

            public ReadWriter(Reader r, Writer w)
            {
                _r = r;
                _w = w;
            }

            public int Read(byte[] buffer, int offset, int count) => _r.Read(buffer, offset, count);
            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _r.ReadAsync(buffer, offset, count, cancellationToken);
            public int Write(byte[] buffer, int offset, int count) => _w.Write(buffer, offset, count);
            public Task<int> WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _w.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }
}
