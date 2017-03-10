using System;
using System.IO;
using System.Linq;
using LibP2P.Utilities.Extensions;
using ProtoBuf;

namespace LibP2P.Crypto
{
    public abstract class PublicKey : Key
    {
        /// <summary>
        /// Verify the signature of data
        /// </summary>
        /// <param name="data">input data</param>
        /// <param name="signature">signature</param>
        /// <returns>validity</returns>
        public abstract bool Verify(byte[] data, byte[] signature);

        /// <summary>
        /// Deserialize a public key from stream
        /// </summary>
        /// <param name="stream">input stream</param>
        /// <returns>public key</returns>
        public static PublicKey Unmarshal(Stream stream)
        {
            var pb = Serializer.Deserialize<PublicKeyContract>(stream);
            switch (pb.Type)
            {
                case KeyType.RSA:
                    return RsaPublicKey.Unmarshal(pb.Data);
                case KeyType.Ed25519:
                    return new Ed25519PublicKey(pb.Data.Take(32).ToArray());
                default:
                    throw new Exception("Bad key type");
            }
        }

        /// <summary>
        /// Deserialize a public key from bytes
        /// </summary>
        /// <param name="data">input bytes</param>
        /// <returns>public key</returns>
        public static PublicKey Unmarshal(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return Unmarshal(stream);
            }
        }

        /// <summary>
        /// Serialize the public key to bytes
        /// </summary>
        /// <returns>serialized bytes</returns>
        public byte[] Marshal() => new PublicKeyContract { Type = Type, Data = MarshalKey() }.SerializeToBytes();

        protected abstract byte[] MarshalKey();
    }
}