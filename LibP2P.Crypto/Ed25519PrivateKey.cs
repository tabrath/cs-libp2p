using System;
using LibP2P.Utilities.Extensions;

namespace LibP2P.Crypto
{
    public class Ed25519PrivateKey : PrivateKey
    {
        private readonly byte[] _sk;
        private readonly byte[] _pk;

        public override KeyType Type => KeyType.Ed25519;
        public override byte[] Bytes => Marshal();
        
        public Ed25519PrivateKey(byte[] sk, byte[] pk = null)
        {
            _sk = sk;
            _pk = pk ?? Sodium.PublicKeyAuth.ExtractEd25519PublicKeyFromEd25519SecretKey(_sk);
        }

        public new static PrivateKey Unmarshal(byte[] data)
        {
            if (data.Length != 96)
                throw new Exception("invalid length");

            var priv = data.Slice(0, 64);
            var pub = data.Slice(64, 32);

            return new Ed25519PrivateKey(priv, pub);
        }

        public override byte[] Sign(byte[] data) =>  Sodium.PublicKeyAuth.SignDetached(data, _sk);
        public override PublicKey GetPublic() => new Ed25519PublicKey(_pk);

        protected override byte[] MarshalKey() => _sk.Append(_pk);
    }
}