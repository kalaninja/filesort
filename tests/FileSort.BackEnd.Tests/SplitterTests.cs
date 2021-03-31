using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileSort.BackEnd.Tests.Utils;
using Shouldly;
using Xunit;

namespace FileSort.BackEnd.Tests
{
    public class SplitterTests
    {
        [Theory]
        [InlineData(10, 4)]
        [InlineData(100, 2)]
        [InlineData(5, 5)]
        [InlineData(2, 4)]
        public async Task SplitsStreamOfIntsIntoMultipleSortedLazyStreams(int arrayLength, int bufferSize)
        {
            var sourceData = DataGenerator.GenerateIntArray(arrayLength);
            var source = sourceData.ToStream();

            var splitter = new Splitter<int>(bufferSize, SerDe.ForInt32, Comparer<int>.Default);
            var streams = splitter.Split(source);

            var count = 0;
            await foreach (var stream in streams)
            {
                var streamData = arrayLength - count * bufferSize >= bufferSize
                    ? new int[bufferSize]
                    : new int[arrayLength % bufferSize];
                stream.Into(streamData);

                var expected = sourceData
                    .Skip(count * bufferSize)
                    .Take(bufferSize)
                    .OrderBy(x => x);
                streamData.ShouldBe(expected);

                count++;
            }
        }

        [Theory]
        [InlineData(10, 4)]
        [InlineData(90, 1)]
        [InlineData(99, 5)]
        [InlineData(5, 5)]
        [InlineData(2, 4)]
        public async Task SplitsStreamOfLongsIntoMultipleSortedLazyStreams(int arrayLength, int bufferSize)
        {
            var sourceData = DataGenerator.GenerateLongArray(arrayLength);
            var source = sourceData.ToStream();

            var splitter = new Splitter<long>(bufferSize, SerDe.ForInt64, Comparer<long>.Default);
            var streams = splitter.Split(source);

            var count = 0;
            await foreach (var stream in streams)
            {
                var streamData = arrayLength - count * bufferSize >= bufferSize
                    ? new long[bufferSize]
                    : new long[arrayLength % bufferSize];
                stream.Into(streamData);

                var expected = sourceData
                    .Skip(count * bufferSize)
                    .Take(bufferSize)
                    .OrderBy(x => x);
                streamData.ShouldBe(expected);

                count++;
            }
        }
    }
}