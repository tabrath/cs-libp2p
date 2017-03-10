using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace LibP2P.Crypto.Tests
{
    [TestFixture]
    public class Ed25519Tests
    {
        [Test]
        public void TestBasicSignAndVerify()
        {
            var pair = KeyPair.Generate(KeyType.Ed25519);
            var data = Encoding.UTF8.GetBytes("hello! and welcome to some awesome crypto primitives");

            var sig = pair.PrivateKey.Sign(data);
            var ok = pair.PublicKey.Verify(data, sig);
            Assert.That(ok, Is.True);

            data[0] ^= data[0];
            ok = pair.PublicKey.Verify(data, sig);
            Assert.That(ok, Is.False);
        }

        [Test]
        public void TestSignZero()
        {
            var pair = KeyPair.Generate(KeyType.Ed25519);
            var data = new byte[] {};

            var sig = pair.PrivateKey.Sign(data);
            var ok = pair.PublicKey.Verify(data, sig);

            Assert.That(ok, Is.True);
        }

        [Test]
        public void TestMarshalLoop()
        {
            var pair = KeyPair.Generate(KeyType.Ed25519);

            var privB = pair.PrivateKey.Bytes;
            var privNew = PrivateKey.Unmarshal(privB);

            Assert.That(pair.PrivateKey, Is.EqualTo(privNew));
            Assert.That(privNew, Is.EqualTo(pair.PrivateKey));

            var pubB = pair.PublicKey.Bytes;
            var pubNew = PublicKey.Unmarshal(pubB);

            Assert.That(pair.PublicKey, Is.EqualTo(pubNew));
            Assert.That(pubNew, Is.EqualTo(pair.PublicKey));
        }

    }
}
