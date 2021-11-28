using System.IO;

namespace GitDb
{
    public interface IObjectSerializer
    {
        byte[] Serialize<T>(ref T data);
        T Deserialize<T>(ref byte[] buffer);
        T Deserialize<T>(Stream stream);
    }
}