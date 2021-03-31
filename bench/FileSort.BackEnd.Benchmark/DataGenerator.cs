using System;
using System.IO;
using System.Linq;
using MoreLinq;

namespace FileSort.BackEnd.Benchmark
{
    public static class DataGenerator
    {
        internal static void GenerateFile(int count, string fileName)
        {
            var rnd = new Random();

            using var fs = File.Create(fileName);
            using var writer = new BinaryWriter(fs);

            Enumerable
                .Repeat(default(int), count)
                .Select(_ => rnd.Next(int.MinValue, int.MaxValue))
                .ForEach(x => writer.Write(x));
        }
    }
}