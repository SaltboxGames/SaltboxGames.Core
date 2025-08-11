/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace SaltboxGames.Core.Collections
{
    public partial class CircularBuffer<T> : IEnumerable<T>
    {
        // These are internal for zlinq enumerable things;
        internal T[] buffer;
        internal int head;

        private int tail;
        private int count;

        public int Count => count;
        public int Capacity => buffer.Length;
        public bool IsEmpty => count == 0;

        public CircularBuffer(int capacity = 8)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }
            buffer = new T[capacity];
        }
        
        public void Push(T item)
        {
            EnsureCapacity();
            buffer[tail] = item;
            tail = (tail + 1) % buffer.Length;
            count++;
        }

        public void Shift(T item)
        {
            EnsureCapacity();
            head = (head - 1 + buffer.Length) % buffer.Length;
            buffer[head] = item;
            count++;
        }

        public T Pop()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            tail = (tail - 1 + buffer.Length) % buffer.Length;
            T value = buffer[tail];
            buffer[tail] = default!;
            count--;
            return value;
        }

        public T Unshift()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            T value = buffer[head];
            buffer[head] = default!;
            head = (head + 1) % buffer.Length;
            count--;
            return value;
        }

        public void PushRange(ReadOnlySpan<T> items)
        {
            EnsureCapacity(items.Length);

            for (int i = 0; i < items.Length; i++)
            {
                buffer[tail] = items[i];
                tail = (tail + 1) % buffer.Length;
            }
            count += items.Length;
        }

        public void ShiftRange(ReadOnlySpan<T> items)
        {
            EnsureCapacity(items.Length);

            for (int i = items.Length - 1; i >= 0; i--)
            {
                head = (head - 1 + buffer.Length) % buffer.Length;
                buffer[head] = items[i];
            }
            count += items.Length;
        }

        public int PopRange(Span<T> destination)
        {
            int toRemove = Math.Min(destination.Length, count);

            for (int i = toRemove - 1; i >= 0; i--)
            {
                tail = (tail - 1 + buffer.Length) % buffer.Length;
                destination[i] = buffer[tail];
                buffer[tail] = default!;
            }

            count -= toRemove;
            return toRemove;
        }

        public int UnshiftRange(Span<T> destination)
        {
            int toRemove = Math.Min(destination.Length, count);

            for (int i = 0; i < toRemove; i++)
            {
                destination[i] = buffer[head];
                buffer[head] = default!;
                head = (head + 1) % buffer.Length;
            }

            count -= toRemove;
            return toRemove;
        }

        
        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return buffer[(head + index) % buffer.Length];
            }
            set
            {
                if ((uint)index >= (uint)count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                buffer[(head + index) % buffer.Length] = value;
            }
        }

        private void EnsureCapacity(int additionalItems = 1)
        {
            if (count + additionalItems <= buffer.Length)
            {
                return;
            }

            int desiredCapacity = Math.Max(buffer.Length * 2, count + additionalItems);
            int offset = (desiredCapacity - count) / 2;
            ReplaceBuffer(desiredCapacity, offset);
        }
        
        public void Justify()
        {
            if (count == 0 || (head == 0 && (tail == count || tail == 0)))
            {
                return;
            }
            
            int offset = (buffer.Length - count) / 2;
            ReplaceBuffer(buffer.Length, offset);
        }
        
        private void ReplaceBuffer(int newCapacity, int offset)
        {
            T[] newBuffer = ArrayPool<T>.Shared.Rent(newCapacity);

            int rightCount = Math.Min(buffer.Length - head, count);
            Array.Copy(buffer, head, newBuffer, offset, rightCount);
            Array.Copy(buffer, 0, newBuffer, offset + rightCount, count - rightCount);

            ArrayPool<T>.Shared.Return(buffer, clearArray: true);
            buffer = newBuffer;
            head = offset;
            tail = offset + count;
        }

        public Span<T> AsSpan()
        {
            Justify();
            return new Span<T>(buffer, head, count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
