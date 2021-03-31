using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FileSort.BackEnd
{
    /// <summary>
    /// Lazy stream over IEnumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class EnumerableStream<T> : Stream
    {
        private readonly IEnumerator<T> _sourceEnumerator;
        private readonly Action<T, byte[]> _serializer;
        private readonly Queue<byte> _buffer = new Queue<byte>();
        private readonly byte[] _byteBuffer = new byte[Marshal.SizeOf<T>()];

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        internal EnumerableStream(IEnumerable<T> source, Action<T, byte[]> serializer)
        {
            _sourceEnumerator = source.GetEnumerator();
            _serializer = serializer;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        private bool SerializeNext()
        {
            if (!_sourceEnumerator.MoveNext())
            {
                return false;
            }

            _serializer(_sourceEnumerator.Current, _byteBuffer);
            foreach (var nextByte in _byteBuffer)
            {
                _buffer.Enqueue(nextByte);
            }

            return true;
        }

        private byte? NextByte()
        {
            if (_buffer.Any() || SerializeNext())
            {
                return _buffer.Dequeue();
            }

            return null;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;

            while (read < count)
            {
                var nextByte = NextByte();
                if (!nextByte.HasValue) break;

                buffer[offset + read] = nextByte.Value;
                read++;
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    internal static class EnumerableStreamExtensions
    {
        internal static EnumerableStream<T> ToStream<T>(this IEnumerable<T> self, Action<T, byte[]> serializer) =>
            new EnumerableStream<T>(self, serializer);
    }
}