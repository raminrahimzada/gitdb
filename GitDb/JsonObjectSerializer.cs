using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace GitDb
{
    public class JsonObjectSerializer : IObjectSerializer
    {
        public byte[] Serialize<T>(ref T data)
        {
            var json = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(ref byte[] buffer)
        {
            var json = Encoding.UTF8.GetString(buffer);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public T Deserialize<T>(Stream stream)
        {
            using (var ms=new MemoryStream())
            {
                stream.CopyTo(ms);
                var buffer = ms.ToArray();
                return Deserialize<T>(ref buffer);
            }
        }
    }
}