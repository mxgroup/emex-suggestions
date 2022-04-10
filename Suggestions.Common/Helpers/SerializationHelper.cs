using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Suggestions.Common.Helpers
{
    public class SerializationHelper
    {
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T ByteArrayToObject<T>(byte[] arrBytes)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                ms.Write(arrBytes, 0, arrBytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                var obj = (T) bf.Deserialize(ms);

                return obj;
            }
        }
    }
}