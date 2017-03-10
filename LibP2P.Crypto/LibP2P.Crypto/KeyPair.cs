using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace LibP2P.Crypto
{
    public class KeyPair
    {
        /// <summary>
        /// The Private Key of the Pair
        /// </summary>
        /// <returns>Private Key</returns>
        public PrivateKey PrivateKey { get; }

        /// <summary>
        /// The Public Key of the Pair
        /// </summary>
        /// <returns>Public Key</returns>
        public PublicKey PublicKey { get; }

        protected KeyPair(PrivateKey privateKey, PublicKey publicKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }

        /// <summary>
        /// Generate a new key pair
        /// </summary>
        /// <param name="type">Key type</param>
        /// <param name="bits">Bits (optional)</param>
        /// <returns>A newly generated key pair</returns>
        public static KeyPair Generate(KeyType type, int? bits = null)
        {
            switch (type)
            {
                case KeyType.RSA:
                    return GenerateRsaKeyPair(bits);
                case KeyType.Ed25519:
                    return GenerateEd25519KeyPair();
                default:
                    throw new NotSupportedException();
            }
        }

        private static KeyPair GenerateRsaKeyPair(int? bits)
        {
            var generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(), bits ?? 512));
            var pair = generator.GenerateKeyPair();
            var priv = (RsaPrivateCrtKeyParameters)pair.Private;
            var pub = (RsaKeyParameters)pair.Public;
            var pk = new RsaPublicKey(pub);
            var sk = new RsaPrivateKey(priv, pub);
            return new KeyPair(sk, pk);
        }

        private static KeyPair GenerateEd25519KeyPair()
        {
            using (var pair = Sodium.PublicKeyAuth.GenerateKeyPair())
            {
                var pk = new Ed25519PublicKey(pair.PublicKey);
                var sk = new Ed25519PrivateKey(pair.PrivateKey, pair.PublicKey);
                return new KeyPair(sk, pk);
            }
        }
    }
}