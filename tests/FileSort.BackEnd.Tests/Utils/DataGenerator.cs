using System;
using System.Linq;

namespace FileSort.BackEnd.Tests.Utils
{
    public static class DataGenerator
    {
        public static int[] GenerateIntArray(int length, int min = int.MinValue, int max = int.MaxValue)
        {
            var rnd = new Random();
            return Enumerable
                .Repeat(default(int), length)
                .Select(_ => rnd.Next(min, max))
                .ToArray();
        }

        public static long[] GenerateLongArray(int length, long min = long.MinValue, long max = long.MaxValue)
        {
            var rnd = new Random();
            return Enumerable
                .Repeat(default(int), length)
                .Select(_ => rnd.NextLong(min, max))
                .ToArray();
        }

        private static long NextLong(this Random self, long min, long max)
        {
            var buf = new byte[8];
            self.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (max - min)) + min;
        }
    }
}