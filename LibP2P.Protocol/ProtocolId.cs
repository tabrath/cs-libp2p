using System;

namespace LibP2P.Protocol
{
    public class ProtocolId : IEquatable<ProtocolId>
    {
        public static readonly ProtocolId TestingId = new ProtocolId("/p2p/_testing");

        private readonly string _value;

        public ProtocolId(string value)
        {
            _value = value;
        }

        public override string ToString() => _value;
        public override bool Equals(object obj) => obj is ProtocolId && Equals((ProtocolId) obj);
        public bool Equals(ProtocolId other) => _value.Equals(other?._value);
        public override int GetHashCode() => _value.GetHashCode();

        public static implicit operator ProtocolId(string s) => new ProtocolId(s);
        public static implicit operator string(ProtocolId id) => id._value;
    }
}
