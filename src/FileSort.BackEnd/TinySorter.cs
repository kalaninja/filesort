using System.IO;
using System.Runtime.InteropServices;

namespace FileSort.BackEnd
{
    public class TinySorter<T> where T : struct
    {
        private const string Split1FileName = "split1";
        private const string Split2FileName = "split2";
        private const string MergeFileName = "merge";

        private readonly GenericSorterConfig<T> _config;
        private readonly string _split1FilePath;
        private readonly string _split2FilePath;
        private readonly string _mergeFilePath;
        private readonly int _itemSize;
        private readonly byte[] _buffer;

        public TinySorter(GenericSorterConfig<T> config)
        {
            _config = config;

            _split1FilePath = Path.Combine(config.TempPath, Split1FileName);
            _split2FilePath = Path.Combine(config.TempPath, Split2FileName);
            _mergeFilePath = Path.Combine(config.TempPath, MergeFileName);
            _itemSize = Marshal.SizeOf<T>();
            _buffer = new byte[_itemSize];
        }

        public void Sort(string sourceFilePath, string destinationPath)
        {
            var seriesSize = 1;

            using (var split1Stream = new FileStream(_split1FilePath, FileMode.Create, FileAccess.ReadWrite))
            using (var split2Stream = new FileStream(_split2FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                int totalSeries;
                using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    totalSeries = Split(sourceStream, split1Stream, split2Stream, seriesSize);
                }

                split1Stream.Rewind();
                split2Stream.Rewind();

                if (totalSeries > 2)
                {
                    using (var mergeStream =
                        new FileStream(_mergeFilePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        do
                        {
                            Merge(split1Stream, split2Stream, mergeStream, seriesSize);
                            split1Stream.Rewind(true);
                            split2Stream.Rewind(true);
                            mergeStream.Rewind();

                            seriesSize <<= 1;
                            totalSeries = Split(mergeStream, split1Stream, split2Stream, seriesSize);
                            split1Stream.Rewind();
                            split2Stream.Rewind();
                            mergeStream.Rewind();
                        } while (totalSeries > 2);
                    }

                    File.Delete(_mergeFilePath);
                }

                using (var destinationStream = File.Create(destinationPath))
                {
                    Merge(split1Stream, split2Stream, destinationStream, seriesSize);
                }
            }

            File.Delete(_split1FilePath);
            File.Delete(_split2FilePath);
        }

        private int Split(Stream source, Stream left, Stream right, int seriesSize)
        {
            var currentSeriesSize = 0;
            var currentStream = left;
            var totalSeries = 1;
            for (var bytesRead = source.Read(_buffer, 0, _itemSize); bytesRead > 0; bytesRead = source.Read(_buffer, 0, _itemSize))
            {
                if (currentSeriesSize == seriesSize)
                {
                    currentStream = currentStream == left ? right : left;
                    currentSeriesSize = 0;
                    totalSeries++;
                }

                currentStream.Write(_buffer, 0, _itemSize);
                currentSeriesSize++;
            }

            return totalSeries;
        }

        private void Merge(Stream left, Stream right, Stream destination, int seriesSize)
        {
            var leftItem = ReadOne(left);
            var rightItem = ReadOne(right);

            while (leftItem.HasValue)
            {
                var leftIdx = 0;
                var rightIdx = 0;

                while (rightIdx < seriesSize && rightItem.HasValue)
                {
                    if (leftIdx < seriesSize && _config.Comparer.Compare(leftItem.Value, rightItem.Value) <= 0)
                    {
                        WriteOne(destination, leftItem.Value);
                        leftIdx++;
                        leftItem = ReadOne(left);
                    }
                    else
                    {
                        WriteOne(destination, rightItem.Value);
                        rightIdx++;
                        rightItem = ReadOne(right);
                    }
                }

                while (leftIdx < seriesSize && leftItem.HasValue)
                {
                    WriteOne(destination, leftItem.Value);
                    leftIdx++;
                    leftItem = ReadOne(left);
                }
            }
        }

        private void WriteOne(Stream stream, T item)
        {
            _config.SerDe.Serializer(item, _buffer);
            stream.Write(_buffer);
        }

        private T? ReadOne(Stream stream)
        {
            var bytesRead = stream.Read(_buffer, 0, _itemSize);
            return bytesRead == 0
                ? default(T?)
                : _config.SerDe.Deserializer(_buffer);
        }
    }
}