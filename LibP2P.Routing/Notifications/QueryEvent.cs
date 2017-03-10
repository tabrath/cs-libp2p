using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibP2P.Peer;
using LibP2P.Peer.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibP2P.Routing.Notifications
{
    public class QueryEvent
    {
        [JsonConverter(typeof(PeerIdConverter))]
        public PeerId Id { get; }
        public QueryEventType Type { get; }
        public PeerInfo[] Responses { get; }
        public string Extra { get; }

        public QueryEvent(PeerId id, QueryEventType type, IEnumerable<PeerInfo> responses, string extra = null)
        {
            Id = id;
            Type = type;
            Responses = responses.ToArray();
            Extra = extra ?? string.Empty;
        }

        private static readonly JsonConverter[] _converters = {new PeerIdConverter(), new PeerInfoJsonConverter()};

        public byte[] MarshalJson()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.None, _converters);
            return Encoding.UTF8.GetBytes(json);
        }

        public static QueryEvent UnmarshalJson(byte[] buffer, int offset = 0, int? count = null)
        {
            return JsonConvert.DeserializeObject<QueryEvent>(Encoding.UTF8.GetString(buffer, offset, count ?? buffer.Length - offset), _converters);
        }

        private class PeerInfoJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var pi = (PeerInfo) value;
                var json = Encoding.UTF8.GetString(pi.MarshalJson());
                writer.WriteRawValue(json);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var obj = JObject.Load(reader);
                var raw = JsonConvert.SerializeObject(obj, _converters);
                return PeerInfo.UnmarshalJson(Encoding.UTF8.GetBytes(raw));
            }

            public override bool CanConvert(Type objectType) => objectType == typeof(PeerInfo);
        }
    }
}