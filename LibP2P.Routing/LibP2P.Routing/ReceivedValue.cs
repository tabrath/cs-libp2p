using LibP2P.Peer;

namespace LibP2P.Routing
{
    public class ReceivedValue
    {
        public PeerId From { get; }
        public byte[] Value { get; }

        public ReceivedValue(PeerId from, byte[] value)
        {
            From = from;
            Value = value;
        }
    }
}