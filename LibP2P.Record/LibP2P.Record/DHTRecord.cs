using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibP2P.Crypto;
using ProtoBuf;

namespace LibP2P.Record
{
    [ProtoContract]
    public class DHTRecord
    {
        [ProtoMember(1)]
        public string Key { get; set; } = string.Empty;

        [ProtoMember(2)]
        public byte[] Value { get; set; } = Array.Empty<byte>();

        [ProtoMember(3)]
        public string Author { get; set; } = string.Empty;

        [ProtoMember(4)]
        public byte[] Signature { get; set; } = Array.Empty<byte>();

        [ProtoMember(5)]
        public string TimeReceived { get; set; } = string.Empty;


        protected DHTRecord()
        {
        }

        public DHTRecord(PrivateKey sk, string key, byte[] value, bool sign)
        {
            Key = key;
            Value = value;
            Author = Encoding.UTF8.GetString(sk.GetPublic().Hash);

            if (sign)
                Signature = sk.Sign(GetBlob());
        }

        public byte[] GetBlob() => Encoding.UTF8.GetBytes(Key)
            .Concat(Value)
            .Concat(Encoding.UTF8.GetBytes(Author))
            .ToArray();
    }

}
