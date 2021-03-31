using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MoreLinq;

namespace FileSort.BackEnd
{
    /// <summary>
    /// Splits a stream into a lazy collection of ordered streams
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Splitter<T> where T : struct
    {
        private readonly int _bufferSize;
        private readonly SerDe<T> _serde;
        private readonly IComparer<T> _comparer;
        private readonly int _itemSize;

        internal Splitter(int bufferSize, SerDe<T> serde, IComparer<T> comparer)
        {
            _bufferSize = bufferSize;
            _serde = serde;
            _comparer = comparer;
            _itemSize = Marshal.SizeOf<T>();
        }

        internal async IAsyncEnumerable<Stream> Split(Stream source)
        {
            var buffer = new byte[_bufferSize * _itemSize];
            while (true)
            {
                var bytesRead = await source.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    yield break;
                }

                // implements an async generator
                yield return buffer
                    .Take(bytesRead)
                    .Batch(_itemSize, _serde.Deserializer)
                    .OrderBy(x => x, _comparer)
                    .ToStream(_serde.Serializer);
            }
        }
    }
}