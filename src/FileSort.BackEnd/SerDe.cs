using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort.BackEnd
{
    public class SerDe<T>
    {
        public Action<T, byte[]> Serializer { get; set; }

        public Func<IEnumerable<byte>, T> Deserializer { get; set; }
    }

    public static class SerDe
    {
        public static SerDe<int> ForInt32 { get; } =
            new SerDe<int>
            {
                Serializer = (x, receiver) => BitConverter.TryWriteBytes(receiver, x),
                Deserializer = x => BitConverter.ToInt32(x.ToArray(), 0)
            };

        public static SerDe<long> ForInt64 { get; } =
            new SerDe<long>
            {
                Serializer = (x, receiver) => BitConverter.TryWriteBytes(receiver, x),
                Deserializer = x => BitConverter.ToInt64(x.ToArray(), 0)
            };
    }
}