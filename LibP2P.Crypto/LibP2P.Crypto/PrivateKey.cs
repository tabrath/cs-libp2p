using System;
using System.IO;
using LibP2P.Utilities.Extensions;
using ProtoBuf;

namespace LibP2P.Crypto
{
    public abstract class PrivateKey : Key
    {
        /// <summary>
        /// Get the signature of given data using this private key
        /// </summary>
        /// <param name="data">data to sign</param>
        /// <returns>signature</returns>
        public abstract byte[] Sign(byte[] data);

        /// <summary>
        /// Get the public key part of the key
        /// </summary>
        /// <returns>public key</returns>
        public abstract PublicKey GetPublic();

        /// <summary>
        /// Deserialize a private key from a stream
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns>private key</returns>
        public static PrivateKey Unmarshal(Stream stream)
        {
            var pb = Serializer.Deserialize<PrivateKeyContract>(stream);

            switch (pb.Type)
            {
                case KeyType.RSA:
                    return RsaPrivateKey.Unmarshal(pb.Data);
                case KeyType.Ed25519:
                    return Ed25519PrivateKey.Unmarshal(pb.Data);
                default:
                    throw new Exception("Bad key type");
            }
        }

        /// <summary>
        /// Deserialize a private key from bytes
        /// </summary>
        /// <param name="data">bytes to read from</param>
        /// <returns>private key</returns>
        public static PrivateKey Unmarshal(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return Unmarshal(stream);
            }
        }

        /// <summary>
        /// Serialize this key to a byte array
        /// </summary>
        /// <returns>serialized bytes</returns>
        public byte[] Marshal() => new PrivateKeyContract { Type = Type, Data = MarshalKey() }.SerializeToBytes();

        protected abstract byte[] MarshalKey();
    }
}