using System;
using System.Runtime.Serialization;
using System.Text;
using Multiformats.Address;
using Multiformats.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibP2P.Peer.Store
{
    public class PeerInfo
    {
        [JsonProperty("ID", ItemConverterType = typeof(PeerIdConverter))]
        public PeerId Id { get; internal set; }

        [JsonProperty("Addrs", ItemConverterType = typeof(MultiaddressConverter))]
        public Multiaddress[] Addresses { get; internal set; }

        private static readonly JsonSerializerSettings _jsonSettings;

        static PeerInfo()
        {
            _jsonSettings = new JsonSerializerSettings();
            _jsonSettings.Converters.Add(new PeerIdConverter());
            _jsonSettings.Converters.Add(new MultiaddressConverter());
            _jsonSettings.Formatting = Formatting.None;
            _jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
        }

        [JsonConstructor]
        internal PeerInfo() { }

        public PeerInfo(PeerId id, Multiaddress[] addresses)
        {
            Id = id;
            Addresses = addresses;
        }

        public byte[] MarshalJson() => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this, _jsonSettings));
        public static PeerInfo UnmarshalJson(byte[] b) => JsonConvert.DeserializeObject<PeerInfo>(Encoding.UTF8.GetString(b), _jsonSettings);
    }

    public class MultiaddressConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType) => objectType == typeof(Multiaddress);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                serializer.Serialize(writer, ((Multiaddress)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Null:
                    return null;
                case JTokenType.String:
                    return Multiaddress.Decode((string)token);
                case JTokenType.Bytes:
                    return Multiaddress.Decode((byte[]) token);
                case JTokenType.Object:
                    var value = (string) token["$value"];
                    return value == null ? null : Multiaddress.Decode(value);
                default:
                    throw new SerializationException("Unknown Multiaddress format");
            }
        }
    }

    public class PeerIdConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType) => objectType == typeof(PeerId);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                serializer.Serialize(writer, ((PeerId)value).ToString(Multibase.Base58));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var token = JToken.Load(reader);
            switch (token.Type)
            {
                case JTokenType.Null:
                    return null;
                case JTokenType.String:
                    return new PeerId(Multibase.DecodeRaw(Multibase.Base58, (string)token));
                case JTokenType.Bytes:
                    return new PeerId((byte[])token);
                case JTokenType.Object:
                    var value = (string)token["$value"];
                    return value == null ? null : new PeerId(Multibase.DecodeRaw(Multibase.Base58, value));
                default:
                    throw new SerializationException("Unknown PeerId format");
            }
        }

    }
}