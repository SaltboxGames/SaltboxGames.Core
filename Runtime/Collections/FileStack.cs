// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SaltboxGames.Core.Collections
{
    public sealed unsafe partial class FileStack<T> : IDisposable, ICollection, IReadOnlyCollection<T>, IEnumerable<T>
        where T : unmanaged
    {
        private const int enumerator_buffer_size = 4096;
        
        private static readonly int elementSize = sizeof(T);
        private static readonly int chunkBufferCapacity = Math.Max(1, enumerator_buffer_size / sizeof(T));

        private readonly FileStream stream;
        private int version;

        public long Count => stream.Length / sizeof(T);
        int ICollection.Count => checked((int)Count);
        int IReadOnlyCollection<T>.Count => checked((int)Count);

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;
        
        public FileStack(string path)
        {
            string fullPath = Path.GetFullPath(path);
            stream = new FileStream(
                fullPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.Read
            );

            if (stream.Length % elementSize != 0)
            {
                stream.Dispose();
                throw new InvalidDataException($"File length is not aligned to {typeof(T).Name}.");
            }
        }

        private FileStack(FileStream stream)
        {
            this.stream = stream;
        }

        public void Push(in T value)
        {
            stream.Position = stream.Length;

            Span<T> valueSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in value), 1);
            stream.Write(MemoryMarshal.AsBytes(valueSpan));
            version++;
        }

        public T Pop()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            T value = Peek();

            stream.SetLength(stream.Length - elementSize);
            stream.Position = stream.Length;
            version++;

            return value;
        }

        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            return Read(Count - 1);
        }

        private T Read(long index)
        {
            if ((ulong)index >= (ulong)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            T value = default;
            Span<T> valueSpan = MemoryMarshal.CreateSpan(ref value, 1);

            stream.Position = index * elementSize;
            ReadExactly(stream, MemoryMarshal.AsBytes(valueSpan));

            return value;
        }
        
        private void Read(long index, Span<T> destination)
        {
            long count = Count;

            if ((ulong)index > (ulong)count || destination.Length > count - index)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            stream.Position = checked(index * elementSize);
            ReadExactly(stream, MemoryMarshal.AsBytes(destination));
        }

        public void Truncate(long count)
        {
            if ((ulong)count > (ulong)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            long length = checked(count * elementSize);

            stream.SetLength(length);
            stream.Position = length;
            version++;
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public void Clear()
        {
            if (stream.Length == 0)
            {
                return;
            }

            stream.SetLength(0);
            stream.Position = 0;
            version++;
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

        private void ThrowIfModified(int expectedVersion)
        {
            if (version != expectedVersion)
            {
                throw new InvalidOperationException("The stack was modified during enumeration.");
            }
        }
        
        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            CopyTo(array.AsSpan(index));
        }
        
        public void CopyTo(Span<T> destination)
        {
            long count = Count;

            if (destination.Length < count)
            {
                throw new ArgumentException("The destination span does not have enough space.", nameof(destination));
            }

            int destinationIndex = 0;
            long remaining = count;

            while (remaining > 0)
            {
                int chunkSize = (int)Math.Min(remaining, chunkBufferCapacity);
                long startIndex = remaining - chunkSize;

                Span<T> chunk = destination.Slice(destinationIndex, chunkSize);

                Read(startIndex, chunk);
                chunk.Reverse();

                destinationIndex += chunkSize;
                remaining -= chunkSize;
            }
        }
        
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException("Only single-dimensional arrays are supported.", nameof(array));
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException("Non-zero lower-bound arrays are not supported.", nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int count = checked((int)Count);
            if (array.Length - index < count)
            {
                throw new ArgumentException("The destination array does not have enough space.", nameof(array));
            }

            if (array is T[] destination)
            {
                CopyTo(destination.AsSpan(index, count));
                return;
            }

            try
            {
                int destinationIndex = index;
                foreach (T value in this)
                {
                    array.SetValue(value, destinationIndex++);
                }
            }
            catch (InvalidCastException exception)
            {
                throw new ArgumentException("The destination array type is not compatible with the stack element type.", nameof(array), exception);
            }
        }
        
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public sealed class Enumerator : IEnumerator<T>
        {
            private readonly FileStack<T> stack;
            private readonly int version;
            private readonly long count;
            private readonly T[] buffer;

            private long remaining;
            private int bufferIndex;
            private T current;

            internal Enumerator(FileStack<T> stack)
            {
                this.stack = stack;
                version = stack.version;
                count = stack.Count;

                int capacity = (int)Math.Min(count, chunkBufferCapacity);

                buffer = new T[capacity];

                remaining = count;
                bufferIndex = 0;
                current = default;
            }

            public T Current => current;

            object IEnumerator.Current => current;

            public bool MoveNext()
            {
                stack.ThrowIfModified(version);

                if (bufferIndex == 0)
                {
                    if (remaining == 0)
                    {
                        current = default;
                        return false;
                    }

                    FillBuffer();
                }

                current = buffer[--bufferIndex];
                remaining--;

                return true;
            }

            public void Reset()
            {
                stack.ThrowIfModified(version);

                remaining = count;
                bufferIndex = 0;
                current = default;
            }

            public void Dispose()
            {
            }

            private void FillBuffer()
            {
                int readCount = (int)Math.Min(
                    buffer.Length,
                    remaining);

                long startIndex = remaining - readCount;

                stack.Read(startIndex, buffer.AsSpan(0, readCount));
                bufferIndex = readCount;
            }
        }
    }
}
