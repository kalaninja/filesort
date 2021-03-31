using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSort.BackEnd.Tests.Utils;
using Shouldly;
using Xunit;

namespace FileSort.BackEnd.Tests
{
    public class MergeSorterTests
    {
        [Theory]
        [MemberData(nameof(Fuzzer), parameters: 10)]
        public async Task EndToEndTest(int arrayLength, int bufferSize)
        {
            const string sourceFileName = "m-src";
            const string destinationFileName = "m-dest";

            var sourceData = DataGenerator.GenerateIntArray(arrayLength);
            var source = sourceData.ToStream();
            await source.SaveToFile(sourceFileName);

            var sorterConfig = new MergeSorterConfig<int>
            {
                BufferSize = bufferSize,
                Comparer = Comparer<int>.Default,
                SerDe = SerDe.ForInt32,
                TempPath = "."
            };
            var sorter = new MergeSorter<int>(sorterConfig);
            await sorter.Sort(sourceFileName, destinationFileName);

            var expected = sourceData.OrderBy(x => x).ToArray();
            var actual = new int[sourceData.Length];
            await using (var destinationStream = new FileStream(destinationFileName, FileMode.Open, FileAccess.Read))
            {
                destinationStream.Into(actual);
            }

            File.Delete(sourceFileName);
            File.Delete(destinationFileName);

            actual.ShouldBe(expected);
        }

        public static IEnumerable<object[]> Fuzzer(int count)
        {
            var rnd = new Random();
            return Enumerable.Repeat(default(int), count)
                .Select(_ => new object[] {rnd.Next(0, 100), rnd.Next(2, 10)});
        }
    }
}