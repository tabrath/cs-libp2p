using System;
using ProtoBuf;

namespace LibP2P.Protocol.Identify
{
    [ProtoContract]
    public class IdentifyContract
    {
        [ProtoMember(5)]
        public string ProtocolVersion { get; set; } = string.Empty;

        [ProtoMember(6)]
        public string AgentVersion { get; set; } = string.Empty;

        [ProtoMember(1)]
        public byte[] PublicKey { get; set; } = Array.Empty<byte>();

        [ProtoMember(2)]
        public byte[][] ListenAddresses { get; set; } = Array.Empty<byte[]>();

        [ProtoMember(4)]
        public byte[] ObservedAddress { get; set; } = Array.Empty<byte>();

        [ProtoMember(3)]
        public string[] Protocols { get; set; } = Array.Empty<string>();
    }
}
