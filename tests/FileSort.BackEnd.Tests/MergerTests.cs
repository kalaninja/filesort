using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileSort.BackEnd.Tests.Utils;
using Shouldly;
using Xunit;

namespace FileSort.BackEnd.Tests
{
    public class MergerTests
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public async Task MergesMultipleStreamsIntoAsyncStream(int[][] data, int[] expected)
        {
            var streams = data
                .Select(x => (Stream) x.ToStream())
                .ToArray();

            var merger = new Merger<int>(SerDe.ForInt32, Comparer<int>.Default);
            var actual = new List<int>();
            await foreach (var next in merger.Merge(streams))
            {
                actual.Add(next);
            }

            actual.ShouldBe(expected);
        }

        public static IEnumerable<object[]> GetData()
        {
            yield return new object[]
            {
                new[] {new[] {1, 3, 5, 7, 9}, new[] {0, 2, 4, 6}, new[] {2, 22}},
                new[] {0, 1, 2, 2, 3, 4, 5, 6, 7, 9, 22}
            };

            yield return new object[]
            {
                new[] {new[] {1, 3, 5, 7, 9}},
                new[] {1, 3, 5, 7, 9}
            };
        }
    }
}