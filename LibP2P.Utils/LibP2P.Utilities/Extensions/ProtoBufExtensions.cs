using System.IO;
using ProtoBuf;

namespace LibP2P.Utilities.Extensions
{
    public static class ProtoBufExtensions
    {
        public static byte[] SerializeToBytes<T>(this T obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);

                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }
    }
}