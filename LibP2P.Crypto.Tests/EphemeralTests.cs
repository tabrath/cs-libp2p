using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Org.BouncyCastle.Utilities.Encoders;

namespace LibP2P.Crypto.Tests
{
    [TestFixture]
    public class EphemeralTests
    {
        [TestCase("P-256")]
        [TestCase("P-384")]
        [TestCase("P-521")]
        [TestCase("curve25519")]
        public void TestGenerateEKeyPair(string curve)
        {
            var bob = EphemeralKeyPair.Generate(curve);
            var alice = EphemeralKeyPair.Generate(curve);

            var aliceSecret = alice.GenerateSharedKey(bob.PublicKey);
            var bobSecret = bob.GenerateSharedKey(alice.PublicKey);

            Assert.That(aliceSecret, Is.EqualTo(bobSecret));
        }

        [Test]
        public void KeysShouldNotMatchIfDifferentCurves()
        {
            var bob = EphemeralKeyPair.Generate("curve25519");
            var alice = EphemeralKeyPair.Generate("P-256");

            Assert.Throws<ArgumentException>(() =>
            {
                var aliceSecret = alice.GenerateSharedKey(bob.PublicKey);
                var bobSecret = bob.GenerateSharedKey(alice.PublicKey);

                Assert.That(aliceSecret, Is.Not.EqualTo(bobSecret));
            });
        }

        [TestCase("P-256", "04f9a7f6cfe098310b19fac2f507c85fad9edfe918b12d717169acead57bebffe9295a528e90169a22e527ecdb2df95ef5dbf4daa9ce303dbfbaa94f83f11d7f4a")]
        [TestCase("P-384", "043bc740577462d781e46a1c475b50fca3e4e304f37caea1f2e5eaac805482f051614e132c401e17b559a13ea79b936a323d09f42d9147642aaa44e1f8ca9c4ae505163522bf8f8a879c82fc40a43117b15522cb0d5a7f6440408d4874eed7499b")]
        [TestCase("P-521", "04017c1d8575b4c6a724a2bed52a22f66f369a05341566800c9c87a511c796b63e20876c483ea72b93ea8ba4015ee579378911ea8b2b1d7d10efbe8b01ece30813ea0901cd358157369ccfd96412223db87dfe670380a4b4b5bfb66035e21f1a459a2ce0d70a849de166e202fae34c44749a2fb22442d0ee5ba1ec3ee032a69810adb4c7f3")]
        public void Interop_FromGoImpl(string curve, string key)
        {
            var goPub = Hex.Decode(key);
            var csPub = EphemeralKeyPair.Generate(curve);

            Assert.DoesNotThrow(() => csPub.GenerateSharedKey(goPub));
        }

    }
}
