using System.Collections.Generic;
using System.IO;
using Shouldly;

namespace FileSort.BackEnd.Tests.Utils
{
    public static class StreamExtensions
    {
        public static void Into(this Stream self, IList<int> receiver)
        {
            using var binaryReader = new BinaryReader(self);
            for (var i = 0; i < receiver.Count; i++)
            {
                receiver[i] = binaryReader.ReadInt32();
            }

            Should.Throw<EndOfStreamException>(() => binaryReader.ReadInt32());
        }

        public static void Into(this Stream self, IList<long> receiver)
        {
            using var binaryReader = new BinaryReader(self);
            for (var i = 0; i < receiver.Count; i++)
            {
                receiver[i] = binaryReader.ReadInt64();
            }

            Should.Throw<EndOfStreamException>(() => binaryReader.ReadInt64());
        }

        public static MemoryStream ToStream(this int[] self)
        {
            var memoryStream = new MemoryStream(self.Length * sizeof(int));
            var writer = new BinaryWriter(memoryStream);

            foreach (var i in self)
            {
                writer.Write(i);
            }

            writer.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }

        public static MemoryStream ToStream(this long[] self)
        {
            var memoryStream = new MemoryStream(self.Length * sizeof(long));
            var writer = new BinaryWriter(memoryStream);

            foreach (var i in self)
            {
                writer.Write(i);
            }

            writer.Flush();
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}