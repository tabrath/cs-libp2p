using System;
using System.Linq;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;

namespace LibP2P.Utilities.Extensions
{
    public static class ByteArrayExtensions
    {
        public static byte[] Append(this byte[] array, params byte[][] arrays)
        {
            var result = new byte[array.Length + arrays.Sum(b => b.Length)];
            Buffer.BlockCopy(array, 0, result, 0, array.Length);
            var offset = array.Length;
            foreach (var item in arrays)
            {
                Buffer.BlockCopy(item, 0, result, offset, item.Length);
                offset += item.Length;
            }
            return result;
        }

        public static int Compare(this byte[] a, byte[] b)
        {
            if (a.Length > b.Length)
                return 1;

            if (b.Length > a.Length)
                return -1;

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] > b[i])
                    return 1;

                if (b[i] > a[i])
                    return -1;
            }

            return 0;
        }

        public static byte[] XOR(this byte[] a, byte[] b)
        {
            var c = new byte[a.Length];
            for (var i = 0; i < a.Length; i++)
                c[i] = (byte)(a[i] ^ b[i]);
            return c;
        }

        /// <summary>
        /// Computes the hash value using SHA2 256bit
        /// </summary>
        /// <param name="bytes">data to hash</param>
        /// <returns>digest</returns>
        public static byte[] ComputeHash(this byte[] bytes) => Multihash.Sum<SHA2_256>(bytes).Digest;
    }
}
