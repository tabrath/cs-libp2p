using System;
using LibP2P.IO;

namespace LibP2P.Utilities
{
    public static unsafe class Binary
    {
        public const int MaxVarintLength16 = 3;
        public const int MaxVarintLength32 = 5;
        public const int MaxVarintLength64 = 10;

        public static int PutUvarint(byte[] buffer, int offset, ulong value)
        {
            int i = 0;
            fixed (byte* p = &buffer[offset])
            {
                while (value >= 0x80)
                {
                    p[i++] = (byte)(value | 0x80);
                    value >>= 7;
                }
                p[i++] = (byte)value;
            }
            return i;
        }

        public static int PutVarint(byte[] buffer, int offset, long value)
        {
            var ux = (ulong)value << 1;
            if (value < 0)
                ux ^= ux;

            return PutUvarint(buffer, offset, ux);
        }

        public static ulong ReadUvarint(IByteReader r)
        {
            ulong x = 0;
            int s = 0;

            for (var i = 0;; i++)
            {
                var b = r.ReadByte();
                if (b < 0x80)
                {
                    if (i > 9 || i == 9 && b > 1)
                        return x;

                    return x | ((ulong) b << s);
                }
                x |= (ulong) (b & 0x7f) << s;
                s += 7;
            }
        }

        public static long ReadVarint(IByteReader r)
        {
            var ux = ReadUvarint(r);
            var x = (int) (ux >> 1);
            if ((ux & 1) != 0)
                x ^= x;

            return x;
        }

        public static int Uvarint(byte[] buffer, int offset, out ulong value)
        {
            fixed (byte* p = &buffer[offset])
            {
                return Read(p, out value);
            }
        }

        public static int Varint(byte[] buffer, int offset, out long value)
        {
            ulong ux;
            int n = Uvarint(buffer, offset, out ux);
            value = (long) (ux >> 1);
            if ((ux & 1) != 0)
                value ^= value;

            return n;
        }

        private static int Read(byte* buffer, out ulong value)
        {
            value = 0;
            for (int i = 0, s = 0; i < 9; i++, s += 7)
            {
                if (buffer[i] < 0x80)
                {
                    if (i > 9 || i == 9 && buffer[i] > 1)
                    {
                        value = 0;
                        return -(i + 1);
                    }
                    value |= (ulong)(buffer[i] << s);
                    return i + 1;
                }
                value |= (ulong)(buffer[i] & 0x7f) << s;
            }
            value = 0;
            return 0;
        }

        public interface IByteOrder
        {
            ushort Uint16(byte[] b, int offset = 0);
            uint Uint32(byte[] b, int offset = 0);
            ulong Uint64(byte[] b, int offset = 0);
            void PutUint16(byte[] b, int offset, ushort value);
            void PutUint32(byte[] b, int offset, uint value);
            void PutUint64(byte[] b, int offset, ulong value);
        }

        private static void EnsureByteLength(byte[] b, int offset, int length)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (b.Length - offset < length)
                throw new ArgumentOutOfRangeException(nameof(b));
        }

        public static readonly IByteOrder LittleEndian = new LittleEndianByteOrder();

        private class LittleEndianByteOrder : IByteOrder
        {

            public ushort Uint16(byte[] b, int offset = 0)
            {
                EnsureByteLength(b, offset, 2);

                return (ushort) (b[offset] |
                                 b[offset + 1] << 8);
            }

            public void PutUint16(byte[] b, int offset, ushort value)
            {
                EnsureByteLength(b, offset, 2);

                b[offset] = (byte) value;
                b[offset + 1] = (byte) (value >> 8);
            }

            public uint Uint32(byte[] b, int offset = 0)
            {
                EnsureByteLength(b, offset, 4);

                return (uint) (b[offset] |
                               b[offset + 1] << 8 |
                               b[offset + 2] << 16 |
                               b[offset + 3] << 24);
            }

            public void PutUint32(byte[] b, int offset, uint value)
            {
                EnsureByteLength(b, offset, 4);

                b[offset] = (byte) value;
                b[offset + 1] = (byte) (value >> 8);
                b[offset + 2] = (byte) (value >> 16);
                b[offset + 3] = (byte) (value >> 24);
            }

            public ulong Uint64(byte[] b, int offset = 0)
            {
                EnsureByteLength(b, offset, 8);

                return (uint) (b[offset] |
                               b[offset + 1] << 8 |
                               b[offset + 2] << 16 |
                               b[offset + 3] << 24 |
                               b[offset + 4] << 32 |
                               b[offset + 5] << 40 |
                               b[offset + 6] << 48 |
                               b[offset + 7] << 56);
            }

            public void PutUint64(byte[] b, int offset, ulong value)
            {
                EnsureByteLength(b, offset, 8);

                b[offset] = (byte)value;
                b[offset + 1] = (byte)(value >> 8);
                b[offset + 2] = (byte)(value >> 16);
                b[offset + 3] = (byte)(value >> 24);
                b[offset + 4] = (byte)(value >> 32);
                b[offset + 5] = (byte)(value >> 40);
                b[offset + 6] = (byte)(value >> 48);
                b[offset + 7] = (byte)(value >> 56);
            }

            public override string ToString() => "LittleEndian";
        }

        public static readonly IByteOrder BigEndian = new BigEndianByteOrder();

        private class BigEndianByteOrder : IByteOrder
        {
            public ushort Uint16(byte[] b, int offset = 0)
            {
                EnsureByteLength(b, offset, 2);

                return (ushort)(b[offset + 1] |
                                 b[offset] << 8);
            }

            public void PutUint16(byte[] b, int offset, ushort value)
            {
                EnsureByteLength(b, offset, 2);

                b[offset] = (byte)(value >> 8);
                b[offset + 1] = (byte)value;
            }

            public uint Uint32(byte[] b, int offset = 0)
            {
                EnsureByteLength(b, offset, 4);

                return (uint)(b[offset + 3] |
                               b[offset + 2] << 8 |
                               b[offset + 1] << 16 |
                               b[offset] << 24);
            }

            public void PutUint32(byte[] b, int offset, uint value)
            {
                EnsureByteLength(b, offset, 4);

                b[offset] = (byte)(value >> 24);
                b[offset + 1] = (byte)(value >> 16);
                b[offset + 2] = (byte)(value >> 8);
                b[offset + 3] = (byte)value;
            }

            public ulong Uint64(byte[] b, int offset = 0)
            {
                EnsureByteLength(b, offset, 8);

                return (uint)(b[offset + 7] << 56 |
                               b[offset + 6] << 48 |
                               b[offset + 5] << 40 |
                               b[offset + 4] << 32 |
                               b[offset + 3] << 24 |
                               b[offset + 2] << 16 |
                               b[offset + 1] << 8 |
                               b[offset]);
            }

            public void PutUint64(byte[] b, int offset, ulong value)
            {
                EnsureByteLength(b, offset, 8);

                b[offset] = (byte)(value >> 56);
                b[offset + 1] = (byte)(value >> 48);
                b[offset + 2] = (byte)(value >> 40);
                b[offset + 3] = (byte)(value >> 32);
                b[offset + 4] = (byte)(value >> 24);
                b[offset + 5] = (byte)(value >> 16);
                b[offset + 6] = (byte)(value >> 8);
                b[offset + 7] = (byte)value;
            }

            public override string ToString() => "BigEndian";
        }
    }
}
