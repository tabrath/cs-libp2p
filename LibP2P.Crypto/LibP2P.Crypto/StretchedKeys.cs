using System;
using System.Text;
using LibP2P.Utilities.Extensions;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace LibP2P.Crypto
{
    public class StretchedKeys
    {
        public byte[] IV { get; set; }
        public byte[] MacKey { get; set; }
        public byte[] CipherKey { get; set; }

        public static Tuple<StretchedKeys, StretchedKeys> Generate(string cipherType, string hashType, byte[] secret)
        {
            var cipherKeySize = 0;
            var ivSize = 0;
            switch (cipherType)
            {
                case "AES-128":
                    ivSize = 16;
                    cipherKeySize = 16;
                    break;
                case "AES-256":
                    ivSize = 16;
                    cipherKeySize = 32;
                    break;
                case "Blowfish":
                    ivSize = 8;
                    cipherKeySize = 32;
                    break;
            }
            var hmacKeySize = 20;
            var seed = Encoding.UTF8.GetBytes("key expansion");
            var result = new byte[2 * (ivSize + cipherKeySize + hmacKeySize)];
            var m = new HMac(DigestUtilities.GetDigest(hashType));
            m.Init(new KeyParameter(secret));
            m.BlockUpdate(seed, 0, seed.Length);
            var a = new byte[m.GetMacSize()];
            m.DoFinal(a, 0);

            var j = 0;
            while (j < result.Length)
            {
                m.Reset();
                m.BlockUpdate(a, 0, a.Length);
                m.BlockUpdate(seed, 0, seed.Length);
                var b = new byte[m.GetMacSize()];
                m.DoFinal(b, 0);

                var todo = b.Length;

                if (j + todo > result.Length)
                    todo = result.Length - j;

                Buffer.BlockCopy(b, 0, result, j, todo);

                j += todo;

                m.Reset();
                m.BlockUpdate(a, 0, a.Length);
                m.DoFinal(a, 0);
            }

            var half = result.Length / 2;
            var r1 = result.Slice(0, half);
            var r2 = result.Slice(half);

            var k1 = new StretchedKeys
            {
                IV = r1.Slice(0, ivSize),
                CipherKey = r1.Slice(ivSize, cipherKeySize),
                MacKey = r1.Slice(ivSize + cipherKeySize)
            };

            var k2 = new StretchedKeys
            {
                IV = r2.Slice(0, ivSize),
                CipherKey = r2.Slice(ivSize, cipherKeySize),
                MacKey = r2.Slice(ivSize + cipherKeySize)
            };

            return Tuple.Create(k1, k2);
        }
    }
}