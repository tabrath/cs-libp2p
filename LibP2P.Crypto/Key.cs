using System;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace LibP2P.Crypto
{
    public abstract class Key : IEquatable<Key>
    {
        /// <summary>
        /// Type of key
        /// </summary>
        /// <returns>Key type</returns>
        public abstract KeyType Type { get; }

        /// <summary>
        /// Raw bytes of the key
        /// </summary>
        /// <returns>Key bytes</returns>
        public abstract byte[] Bytes { get; }

        private byte[] _hash;
        /// <summary>
        /// Hash digest of the key
        /// </summary>
        /// <returns>Hash digest</returns>
        public byte[] Hash => _hash ?? (_hash = Utils.Hash(Bytes));

        /// <summary>
        /// String representation of the key in hex format
        /// </summary>
        /// <returns>Hex formatted strinrg</returns>
        public override string ToString() => Hex.ToHexString(Hash);

        /// <summary>
        /// Check the equality of two keys
        /// </summary>
        /// <param name="other">Comparand</param>
        /// <returns>equality</returns>
        public bool Equals(Key other) => other != null && Bytes.SequenceEqual(other.Bytes);
    }
}
