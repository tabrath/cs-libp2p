using System;
using ProtoBuf;

namespace LibP2P.Crypto
{
    [ProtoContract]
    internal class PrivateKeyContract
    {
        [ProtoMember(1, IsRequired = true)]
        public KeyType Type { get; set; } = 0;

        [ProtoMember(2, IsRequired = true)]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}