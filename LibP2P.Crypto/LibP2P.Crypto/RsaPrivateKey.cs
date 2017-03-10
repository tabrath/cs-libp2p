using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace LibP2P.Crypto
{
    public class RsaPrivateKey : PrivateKey
    {
        private readonly RsaPrivateCrtKeyParameters _sk;
        private readonly RsaKeyParameters _pk;
        private readonly Lazy<ISigner> _instance;

        public override KeyType Type => KeyType.RSA;
        public override byte[] Bytes => Marshal();

        public RsaPrivateKey(RsaPrivateCrtKeyParameters sk, RsaKeyParameters pk = null)
        {
            _sk = sk;
            _pk = pk ?? new RsaKeyParameters(false, _sk.Modulus, _sk.PublicExponent);
            _instance = new Lazy<ISigner>(() =>
            {
                var rsa = new RsaDigestSigner(new Sha256Digest());
                rsa.Init(true, _sk);
                return rsa;
            });
        }


        public override byte[] Sign(byte[] data) => _instance.Value.Sign(data);
        public override PublicKey GetPublic() => new RsaPublicKey(_pk);
        protected override byte[] MarshalKey() => Utils.MarshalPKCS1PrivateKey(_sk);

        public new static RsaPrivateKey Unmarshal(byte[] data) => new RsaPrivateKey(Utils.UnmarshalPKCS1PrivateKey(data));
    }
}