using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace LibP2P.Crypto
{
    public class EphemeralKeyPair
    {
        public delegate byte[] GenerateSharedKeyDelegate(byte[] publicKey);

        public byte[] PublicKey { get; }

        private readonly GenerateSharedKeyDelegate _generator;

        public EphemeralKeyPair(byte[] publicKey, GenerateSharedKeyDelegate generator)
        {
            PublicKey = publicKey;
            _generator = generator;
        }

        public byte[] GenerateSharedKey(byte[] publicKey) => _generator(publicKey);

        public static EphemeralKeyPair Generate(string curveName)
        {
            var ecp = CustomNamedCurves.GetByName(curveName);
            var ecs = new ECDomainParameters(ecp.Curve, ecp.G, ecp.N, ecp.H, ecp.GetSeed());
            var g = new ECKeyPairGenerator();
            g.Init(new ECKeyGenerationParameters(ecs, new SecureRandom()));
            
            var pair = g.GenerateKeyPair();
            var agree = new ECDHBasicAgreement();
            agree.Init(pair.Private);
            
            var pubkey = MarshalCurvePoint(((ECPublicKeyParameters)pair.Public).Q);
            var done = new GenerateSharedKeyDelegate(theirPub =>
            {
                var point = UnmarshalCurvePoint(ecp.Curve, theirPub);
                var key = new ECPublicKeyParameters(point, ecs);
                
                return agree.CalculateAgreement(key).ToByteArray();
            });

            return new EphemeralKeyPair(pubkey, done);
        }

        private static byte[] MarshalCurvePoint(ECPoint point) => new X9ECPoint(point, false).GetPointEncoding();
        private static ECPoint UnmarshalCurvePoint(ECCurve curve, byte[] data) => curve.DecodePoint(data);
    }
}