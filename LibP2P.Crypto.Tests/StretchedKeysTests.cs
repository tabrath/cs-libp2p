using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibP2P.Utilities.Extensions;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace LibP2P.Crypto.Tests
{
    [TestFixture]
    public class StretchedKeysTests
    {
        [Test]
        public void CanStretchKeysUsingBCL()
        {
            var ekeypair1 = EphemeralKeyPair.Generate("P-256");
            var ekeypair2 = EphemeralKeyPair.Generate("P-256");
            var secret1 = ekeypair1.GenerateSharedKey(ekeypair2.PublicKey);
            var secret2 = ekeypair2.GenerateSharedKey(ekeypair1.PublicKey);
            var stretched1 = StretchedKeys.Generate("AES-256", "SHA256", secret1);
            var stretched2 = StretchedKeys.Generate("AES-256", "SHA256", secret2);

            var raw = Encoding.UTF8.GetBytes("Hello world, this should be encrypted.");
            byte[] encoded = null;
            byte[] decoded = null;

            using (var aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using (var encryptor = aes.CreateEncryptor(stretched1.Item1.CipherKey, stretched1.Item1.IV))
                using (var mac = HMAC.Create("HMACSHA256"))
                {
                    mac.Initialize();
                    mac.Key = stretched1.Item1.MacKey;
                    var data = encryptor.TransformFinalBlock(raw, 0, raw.Length);

                    encoded = data
                            .Concat(mac.ComputeHash(data, 0, data.Length))
                            .ToArray();
                }

                using (var decryptor = aes.CreateDecryptor(stretched2.Item1.CipherKey, stretched2.Item1.IV))
                using (var mac = HMAC.Create("HMACSHA256"))
                {
                    mac.Initialize();
                    mac.Key = stretched2.Item1.MacKey;
                    var mark = encoded.Length - (mac.HashSize/8);
                    var digest = encoded.Skip(mark).ToArray();
                    Assert.That(mac.ComputeHash(encoded, 0, mark), Is.EqualTo(digest));

                    decoded = decryptor.TransformFinalBlock(encoded, 0, mark);
                }
            }

            Assert.That(Encoding.UTF8.GetString(decoded), Is.EqualTo(Encoding.UTF8.GetString(raw)));
        }

        [TestCase("P-256", "AES-128", "SHA256")]
        [TestCase("P-256", "AES-256", "SHA256")]
        [TestCase("curve25519", "AES-256", "SHA256")]
        [TestCase("P-256", "AES-256", "SHA512")]
        [TestCase("P-256", "Blowfish", "SHA256")]
        public void CanStretchKeysUsingBouncyCastle(string curve, string cipher, string hash)
        {
            var ekeypair1 = EphemeralKeyPair.Generate(curve);
            var ekeypair2 = EphemeralKeyPair.Generate(curve);
            var secret1 = ekeypair1.GenerateSharedKey(ekeypair2.PublicKey);
            var secret2 = ekeypair2.GenerateSharedKey(ekeypair1.PublicKey);
            var stretched1 = StretchedKeys.Generate(cipher, hash, secret1);
            var stretched2 = StretchedKeys.Generate(cipher, hash, secret2);

            var raw = Encoding.UTF8.GetBytes("Hello world, this should be encrypted.");
            byte[] encoded = null;
            byte[] decoded = null;

            cipher = cipher.Split('-').First();
            hash = "HMAC" + hash;

            var cipherKey1 = new ParametersWithIV(ParameterUtilities.CreateKeyParameter(cipher, stretched1.Item1.CipherKey), stretched1.Item1.IV);
            var cipherKey2 = new ParametersWithIV(ParameterUtilities.CreateKeyParameter(cipher, stretched2.Item1.CipherKey), stretched2.Item1.IV);

            var encryptor = CipherUtilities.GetCipher(cipher + "/CTR/NoPadding");
            encryptor.Init(true, cipherKey1);

            encoded = encryptor.DoFinal(raw);
            encoded = encoded.Append(MacUtilities.CalculateMac(hash, new KeyParameter(stretched1.Item1.MacKey), encoded));

            var decryptor = CipherUtilities.GetCipher(cipher + "/CTR/NoPadding");
            decryptor.Init(false, cipherKey2);

            var mac = MacUtilities.GetMac(hash);
            mac.Init(new KeyParameter(stretched2.Item1.MacKey));
            var digest = encoded.Slice(encoded.Length - mac.GetMacSize());
            Assert.That(MacUtilities.DoFinal(mac, encoded.Slice(0, encoded.Length - digest.Length)), Is.EqualTo(digest));

            decoded = decryptor.DoFinal(encoded, 0, encoded.Length - digest.Length);
            Assert.That(Encoding.UTF8.GetString(decoded), Is.EqualTo(Encoding.UTF8.GetString(raw)));
        }

        [Test]
        public void GoInterop()
        {
            var k1 = new
            {
                iv = new byte[] {208, 132, 203, 169, 253, 52, 40, 83, 161, 91, 17, 71, 33, 136, 67, 96},
                cipherKey =
                new byte[]
                {
                    156, 48, 241, 157, 92, 248, 153, 186, 114, 127, 195, 114, 106, 104, 215, 133, 35, 11, 131, 137, 123,
                    70, 74, 26, 15, 60, 189, 32, 67, 221, 115, 137
                },
                macKey =
                new byte[] {6, 179, 91, 245, 224, 56, 153, 120, 77, 140, 29, 5, 15, 213, 187, 65, 137, 230, 202, 120}
            };
            var k2 = new
            {
                iv = new byte[] { 236, 17, 34, 141, 90, 106, 197, 56, 197, 184, 157, 135, 91, 88, 112, 19 },
                cipherKey = new byte[] { 151, 145, 195, 219, 76, 195, 102, 109, 187, 231, 100, 150, 132, 245, 251, 130, 254, 37, 178, 55, 227, 34, 114, 39, 238, 34, 2, 193, 107, 130, 32, 87 },
                macKey = new byte[] { 3, 229, 77, 212, 241, 217, 23, 113, 220, 126, 38, 255, 18, 117, 108, 205, 198, 89, 1, 236 }
            };

            var keys = StretchedKeys.Generate("AES-256", "SHA256", new byte[] { 195, 191, 209, 165, 209, 201, 127, 122, 136, 111, 31, 66, 111, 68, 38, 155, 216, 204, 46, 181, 200, 188, 170, 204, 104, 74, 239, 251, 173, 114, 222, 234 });

            Assert.That(keys.Item1.IV, Is.EqualTo(k1.iv));
            Assert.That(keys.Item1.CipherKey, Is.EqualTo(k1.cipherKey));
            Assert.That(keys.Item1.MacKey, Is.EqualTo(k1.macKey));

            Assert.That(keys.Item2.IV, Is.EqualTo(k2.iv));
            Assert.That(keys.Item2.CipherKey, Is.EqualTo(k2.cipherKey));
            Assert.That(keys.Item2.MacKey, Is.EqualTo(k2.macKey));
        }
    }
}
