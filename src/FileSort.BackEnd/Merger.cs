using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileSort.BackEnd
{
    /// <summary>
    /// Merges multiple streams into a single async stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Merger<T> where T : struct
    {
        private readonly SerDe<T> _serde;
        private readonly IComparer<T> _comparer;

        public Merger(SerDe<T> serde, IComparer<T> comparer)
        {
            _serde = serde;
            _comparer = comparer;
        }

        public async IAsyncEnumerable<T> Merge(Stream[] streams)
        {
            // populate buffer in parallel
            var buffer = await Task.WhenAll(
                streams.Select(x => x.ReadOneAsync(_serde.Deserializer)));

            var index = IndexOfNextElement(buffer);
            while (index.HasValue)
            {
                yield return buffer[index.Value].Value;
                buffer[index.Value] = await streams[index.Value].ReadOneAsync(_serde.Deserializer);
                index = IndexOfNextElement(buffer);
            }
        }

        private int? IndexOfNextElement(IReadOnlyList<T?> buffer)
        {
            var index = 0;
            while (true)
            {
                if (index >= buffer.Count) return null;
                if (buffer[index].HasValue) break;
                index++;
            }

            var min = buffer[index].Value;
            for (var i = index + 1; i < buffer.Count; i++)
            {
                if (buffer[i].HasValue && _comparer.Compare(buffer[i].Value, min) < 0)
                {
                    index = i;
                    min = buffer[i].Value;
                }
            }

            return index;
        }
    }
}