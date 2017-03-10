using System;
using System.Linq;
using System.Text;
using KeySpace;
using LibP2P.Peer;
using LibP2P.Utilities.Extensions;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;

namespace LibP2P.KBucket
{
    public class DhtId : IComparable<DhtId>
    {
        private readonly byte[] _bytes;

        protected DhtId(byte[] bytes)
        {
            _bytes = bytes;
        }

        public int CompareTo(DhtId other)
        {
            var a = new Key(XORKeySpace.Instance, this);
            var b = new Key(XORKeySpace.Instance, other);
            return a.CompareTo(b);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DhtId;
            return other != null && _bytes.SequenceEqual(other._bytes);
        }

        public static implicit operator DhtId(byte[] bytes) => new DhtId(bytes);
        public static implicit operator byte[](DhtId id) => id._bytes;

        private static DhtId Xor(DhtId a, DhtId b) => ((byte[])a).XOR(b);
        internal static int CommonPrefixLength(DhtId a, DhtId b) => XORKeySpace.ZeroPrefixLength(Xor(a, b));

        public static DhtId ConvertPeerId(PeerId id) => Multihash.Sum<SHA2_256>(id).Digest;
        public static DhtId ConvertKey(string id) => Multihash.Sum<SHA2_256>(Encoding.UTF8.GetBytes(id)).Digest;

        public bool Closer(PeerId a, PeerId b, string key)
        {
            var aid = ConvertPeerId(a);
            var bid = ConvertPeerId(b);
            var tgt = ConvertKey(key);
            var adist = Xor(aid, tgt);
            var bdist = Xor(bid, tgt);

            return adist.CompareTo(bdist) == -1;
        }
    }
}