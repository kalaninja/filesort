using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FileSort.BackEnd
{
    internal class FilePathProvider : IEnumerable<string>
    {
        private readonly string _path;
        private readonly string _prefix;

        internal int Count { get; set; }

        internal string Next
        {
            get
            {
                Count++;
                return this[Count];
            }
        }

        internal string this[int index] => Path.Combine(_path, $"{_prefix}_{index}.tmp");

        internal FilePathProvider(string path, string prefix)
        {
            _path = path;
            _prefix = prefix;
        }

        internal FilePathProvider Fork(string prefix) => new FilePathProvider(_path, prefix);

        public IEnumerator<string> GetEnumerator()
        {
            for (var i = 1; i <= Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}