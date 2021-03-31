using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace FileSort.BackEnd.Benchmark
{
    public class MergeVsTiny
    {
        private const string FileName = "vs";

        public MergeVsTiny()
        {
            DataGenerator.GenerateFile(100, FileName);
        }

        [Benchmark]
        public void TinySorter()
        {
            var sorterConfig = new GenericSorterConfig<int>
            {
                Comparer = Comparer<int>.Default,
                SerDe = SerDe.ForInt32,
                TempPath = "."
            };
            var sorter = new TinySorter<int>(sorterConfig);
            sorter.Sort(FileName, $"dest-{FileName}");
        }

        [Benchmark]
        public async Task MergeSorter()
        {
            var sorterConfig = new MergeSorterConfig<int>
            {
                BufferSize = 4,
                Comparer = Comparer<int>.Default,
                SerDe = SerDe.ForInt32,
                TempPath = "."
            };
            var sorter = new MergeSorter<int>(sorterConfig);
            await sorter.Sort(FileName, $"dest-{FileName}");
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            File.Delete(FileName);
            File.Delete($"dest-{FileName}");
        }
    }
}