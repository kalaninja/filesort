using FileSort.BackEnd.Tests.Utils;
using Shouldly;
using Xunit;

namespace FileSort.BackEnd.Tests
{
    public class EnumerableStreamTests
    {
        [Theory]
        [InlineData]
        [InlineData(1)]
        [InlineData(1, 2, 3)]
        public void StreamShouldWrapUnderlyingEnumerable(params int[] expected)
        {
            var stream = expected.ToStream(SerDe.ForInt32.Serializer);

            var actual = new int[expected.Length];
            stream.Into(actual);

            actual.ShouldBe(expected);
        }
    }
}