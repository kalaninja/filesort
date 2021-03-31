using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace FileSort.BackEnd.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 0, targetCount: 1)]
    public class MemoryAllocations
    {
        private const string ShortFileName = "short";
        private const string LongFileName = "long";

        public MemoryAllocations()
        {
            DataGenerator.GenerateFile(100, ShortFileName);
            DataGenerator.GenerateFile(100000, LongFileName);
        }

        [Benchmark]
        public void ShortFile() => SortFile(ShortFileName);

        [Benchmark]
        public void LongFile() => SortFile(LongFileName);

        private static void SortFile(string fileName)
        {
            var sorterConfig = new GenericSorterConfig<int>
            {
                Comparer = Comparer<int>.Default,
                SerDe = SerDe.ForInt32,
                TempPath = "."
            };
            var sorter = new TinySorter<int>(sorterConfig);
            sorter.Sort(fileName, $"dest-{fileName}");
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            File.Delete(ShortFileName);
            File.Delete(LongFileName);
            File.Delete($"dest-{ShortFileName}");
            File.Delete($"dest-{LongFileName}");
        }
    }
}