using BenchmarkDotNet.Running;

namespace FileSort.BackEnd.Benchmark
{
    static class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<MergeVsTiny>();
        }
    }
}