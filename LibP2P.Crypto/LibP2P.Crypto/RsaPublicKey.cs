using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace LibP2P.Crypto
{
    public class RsaPublicKey : PublicKey
    {
        private readonly RsaKeyParameters _k;
        private readonly Lazy<ISigner> _instance;

        public override KeyType Type => KeyType.RSA;
        public override byte[] Bytes => Marshal();

        public RsaPublicKey(RsaKeyParameters k)
        {
            _k = k;
            _instance = new Lazy<ISigner>(() =>
            {
                var rsa = new RsaDigestSigner(new Sha256Digest());
                rsa.Init(false, _k);
                return rsa;
            });
        }

        public override bool Verify(byte[] data, byte[] signature) => _instance.Value.Verify(data, signature);
        protected override byte[] MarshalKey() => Utils.MarshalPKIXPublicKey(_k);

        public new static RsaPublicKey Unmarshal(byte[] data) => new RsaPublicKey(Utils.UnmarshalPKIXPublicKey(data));
    }
}