using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FileSort.BackEnd
{
    internal static class StreamExtensions
    {
        internal static async Task SaveToFile<T>(this IAsyncEnumerable<T> self, string path, Action<T, byte[]> serializer)
        {
            var itemSize = Marshal.SizeOf<T>();
            var buffer = new byte[itemSize];
            await using var fileStream = File.Create(path);
            await foreach (var item in self)
            {
                serializer(item, buffer);
                await fileStream.WriteAsync(buffer, 0, itemSize);
            }
        }

        internal static async Task SaveToFile(this Stream self, string path)
        {
            await using var fileStream = File.Create(path);
            await self.CopyToAsync(fileStream);
        }

        internal static async Task<T?> ReadOneAsync<T>(this Stream self, Func<IEnumerable<byte>, T> deserializer) where T : struct
        {
            var itemSize = Marshal.SizeOf<T>();
            var buffer = new byte[itemSize];
            var bytesRead = await self.ReadAsync(buffer, 0, buffer.Length);
            return bytesRead == 0
                ? default(T?)
                : deserializer(buffer);
        }

        internal static void Rewind(this FileStream self, bool clear = false)
        {
            if (clear)
            {
                self.SetLength(0);
            }
            else
            {
                self.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}