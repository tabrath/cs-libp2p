using LibP2P.Utilities.Extensions;

namespace LibP2P.Crypto
{
    public class Ed25519PublicKey : PublicKey
    {
        private readonly byte[] _k;
        
        public override KeyType Type => KeyType.Ed25519;
        public override byte[] Bytes => MarshalKey();

        public Ed25519PublicKey(byte[] k)
        {
            _k = k;
        }

        public override bool Verify(byte[] data, byte[] signature) => Sodium.PublicKeyAuth.VerifyDetached(signature, data, _k);

        protected override byte[] MarshalKey() => new PublicKeyContract { Type = Type, Data = _k }.SerializeToBytes();
    }
}