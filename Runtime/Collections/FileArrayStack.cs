using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SaltboxGames.Core.Collections
{
    public sealed unsafe class FileArrayStack<T> : IDisposable
        where T : unmanaged
    {
        private const int footer_size = sizeof(int);
        private static readonly int elementSize = sizeof(T);

        private readonly FileStream stream;

        private long count;
        public long Count => count;

        public int PeekLength => GetTopEntry().Length;

        public FileArrayStack(string path)
        {
            stream = new FileStream(
                Path.GetFullPath(path),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.Read
            );

            try
            {
                count = ValidateAndCount();
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public void Push(ReadOnlySpan<T> values)
        {
            stream.Position = stream.Length;

            if (!values.IsEmpty)
            {
                stream.Write(MemoryMarshal.AsBytes(values));
            }

            int length = values.Length;

            Span<int> footer = MemoryMarshal.CreateSpan(ref length, 1);
            stream.Write(MemoryMarshal.AsBytes(footer));

            count++;
        }

        public void Peek(Span<T> destination)
        {
            Entry entry = GetTopEntry();

            ValidateDestination(destination, entry.Length);
            Read(entry, destination);
        }

        public void Pop(Span<T> destination)
        {
            Entry entry = GetTopEntry();

            ValidateDestination(destination, entry.Length);
            Read(entry, destination);

            stream.SetLength(entry.Start);
            stream.Position = entry.Start;

            count--;
        }

        public void Discard()
        {
            Entry entry = GetTopEntry();

            stream.SetLength(entry.Start);
            stream.Position = entry.Start;

            count--;
        }

        public void Clear()
        {
            if (count == 0)
            {
                return;
            }

            stream.SetLength(0);
            stream.Position = 0;

            count = 0;
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        private Entry GetTopEntry()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            long end = stream.Length;
            int length = ReadLength(end);
            long byteLength = checked((long)length * elementSize);
            long start = end - footer_size - byteLength;

            return new Entry(start, length);
        }

        private void Read(Entry entry, Span<T> destination)
        {
            if (entry.Length == 0)
            {
                return;
            }

            stream.Position = entry.Start;
            ReadExactly(stream, MemoryMarshal.AsBytes(destination[..entry.Length]));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateDestination(Span<T> destination, int length)
        {
            if (destination.Length < length)
            {
                throw new ArgumentException("The destination span does not have enough space.", nameof(destination));
            }
        }

        private long ValidateAndCount()
        {
            long position = stream.Length;
            long result = 0;

            while (position > 0)
            {
                if (position < footer_size)
                {
                    throw new InvalidDataException("File contains an incomplete array footer.");
                }

                int length = ReadLength(position);
                if (length < 0)
                {
                    throw new InvalidDataException("File contains a negative array length.");
                }

                long byteLength;
                try
                {
                    byteLength = checked((long)length * elementSize);
                }
                catch (OverflowException exception)
                {
                    throw new InvalidDataException("File contains an invalid array length.", exception);
                }

                long recordSize;
                try
                {
                    recordSize = checked(byteLength + footer_size);
                }
                catch (OverflowException exception)
                {
                    throw new InvalidDataException("File contains an invalid array length.", exception);
                }

                if (recordSize > position)
                {
                    throw new InvalidDataException("File contains an incomplete array.");
                }

                position -= recordSize;
                result++;
            }

            return result;
        }

        private int ReadLength(long endPosition)
        {
            int length = 0;

            Span<int> footer = MemoryMarshal.CreateSpan(ref length, 1);
            stream.Position = endPosition - footer_size;

            ReadExactly(stream, MemoryMarshal.AsBytes(footer));
            return length;
        }

        private static void ReadExactly(Stream stream, Span<byte> buffer)
        {
            while (!buffer.IsEmpty)
            {
                int read = stream.Read(buffer);
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }

                buffer = buffer[read..];
            }
        }

        private readonly struct Entry
        {
            public long Start { get; }
            public int Length { get; }

            public Entry(long start, int length)
            {
                Start = start;
                Length = length;
            }
        }
    }
}
