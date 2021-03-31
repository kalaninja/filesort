using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace FileSort.BackEnd
{
    public class MergeSorter<T> where T : struct
    {
        private readonly MergeSorterConfig<T> _config;

        public MergeSorter(MergeSorterConfig<T> config)
        {
            _config = config;
        }

        public async Task Sort(string sourceFilePath, string destinationPath)
        {
            var sourcePathProvider = await SplitSourceFileIntoSortedFilesOfBufferSize(sourceFilePath);

            var merger = new Merger<T>(_config.SerDe, _config.Comparer);
            var pathProvider = await RepeatMerging(sourcePathProvider, merger);
            await MergeIntoDestination(destinationPath, pathProvider, merger);
        }

        private async Task MergeIntoDestination(string destinationPath, FilePathProvider pathProvider, Merger<T> merger)
        {
            var streams = pathProvider
                .Select(x => (Stream) new FileStream(x, FileMode.Open, FileAccess.Read))
                .ToArray();
            await merger.Merge(streams).SaveToFile(destinationPath, _config.SerDe.Serializer);

            streams.ForEach(x => x.Dispose());
            pathProvider.ForEach(File.Delete);
        }

        private async Task<FilePathProvider> RepeatMerging(FilePathProvider sourcePathProvider, Merger<T> merger)
        {
            var stage = 0;
            while (sourcePathProvider.Count > _config.BufferSize)
            {
                var destinationPathProvider = sourcePathProvider.Fork($"stage{stage}");

                // Split into groups of buffer size
                var mergeGroups = sourcePathProvider.Batch(_config.BufferSize);

                // merge each group into a single file
                foreach (var filesToMerge in mergeGroups)
                {
                    var streams = filesToMerge
                        .Select(x => (Stream) new FileStream(x, FileMode.Open, FileAccess.Read))
                        .ToArray();

                    await merger
                        .Merge(streams)
                        .SaveToFile(destinationPathProvider.Next, _config.SerDe.Serializer);

                    streams.ForEach(x => x.Dispose());
                }

                // delete all files of this stage
                sourcePathProvider.ForEach(File.Delete);

                sourcePathProvider = destinationPathProvider;
                stage++;
            }

            return sourcePathProvider;
        }

        private async Task<FilePathProvider> SplitSourceFileIntoSortedFilesOfBufferSize(string sourceFilePath)
        {
            var splitter = new Splitter<T>(_config.BufferSize, _config.SerDe, _config.Comparer);
            var splitPathProvider = new FilePathProvider(_config.TempPath, "split");

            await using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);

            var streams = splitter.Split(fileStream);
            await foreach (var stream in streams)
            {
                await stream.SaveToFile(splitPathProvider.Next);
            }

            return splitPathProvider;
        }
    }
}