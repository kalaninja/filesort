using System.Collections.Generic;

namespace FileSort.BackEnd
{
    public class GenericSorterConfig<T> where T : struct
    {
        public SerDe<T> SerDe { get; set; }
        public IComparer<T> Comparer { get; set; }
        public string TempPath { get; set; }
    }

    public class MergeSorterConfig<T> : GenericSorterConfig<T> where T : struct
    {
        public int BufferSize { get; set; }
    }
}