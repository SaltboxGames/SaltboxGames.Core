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
using System.Runtime.CompilerServices;

namespace SaltboxGames.Core.Collections
{
    public sealed partial class CircularBuffer<T> : IEnumerable<T>, IDisposable
    {
        private static readonly bool clear_return_buffer =
            RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        
        // Internal for zlinq enumerable things
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

            buffer = ArrayPool<T>.Shared.Rent(capacity);
        }

        // ---------- helpers (no modulo) ----------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int PhysIdx(int logical)
        {
            int idx = head + logical;
            if ((uint)idx >= (uint)buffer.Length)
            {
                idx -= buffer.Length;
            }
            return idx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Inc(int i)
        {
            int n = i + 1;
            return (n == buffer.Length) ? 0 : n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Dec(int i)
        {
            return (i == 0) ? buffer.Length - 1 : i - 1;
        }

        // ---------- internal single operations ----------
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushUnchecked(T item) 
        {
            buffer[tail] = item;
            tail = Inc(tail);
            count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ShiftUnchecked(T item)
        {
            head = Dec(head);
            buffer[head] = item;
            count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PopUnchecked()
        {
            tail = Dec(tail);
            T v = buffer[tail];
            buffer[tail] = default!;
            count--;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T UnshiftUnchecked()
        {
            T v = buffer[head];
            buffer[head] = default!;
            head = Inc(head);
            count--;
            return v;
        }
        
        // ---------- single operations ----------
        public void Push(T item)
        {
            EnsureCapacity();
            PushUnchecked(item);
        }

        public void Shift(T item)
        {
            EnsureCapacity();
            ShiftUnchecked(item);
        }

        public T Pop()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return PopUnchecked();
        }

        public T Unshift()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return UnshiftUnchecked();
        }

        // ---------- range operations ----------
        public void PushRange(ReadOnlySpan<T> items)
        {
            if (items.Length == 0)
            {
                return;
            }
            EnsureCapacity(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                PushUnchecked(items[i]);
            }
        }

        public void ShiftRange(ReadOnlySpan<T> items)
        {
            if (items.Length == 0)
            {
                return;
            }
            EnsureCapacity(items.Length);
            for (int i = items.Length - 1; i >= 0; i--)
            {
                ShiftUnchecked(items[i]);
            }
        }

        public int PopRange(Span<T> destination)
        {
            int toRemove = Math.Min(destination.Length, count);
            for (int i = toRemove - 1; i >= 0; i--)
            {
                destination[i] = PopUnchecked();
            }
            return toRemove;
        }

        public int UnshiftRange(Span<T> destination)
        {
            int toRemove = Math.Min(destination.Length, count);
            for (int i = 0; i < toRemove; i++)
            {
                destination[i] = UnshiftUnchecked();
            }
            return toRemove;
        }

        // ---------- peeks ----------
        public T PeekHead()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return this[0];
        }

        public T PeekTail()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return this[^1];
        }

        // ---------- indexers ----------
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return buffer[PhysIdx(index)];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint)index >= (uint)count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                buffer[PhysIdx(index)] = value;
            }
        }

        public T this[Index index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int i = index.IsFromEnd ? count - index.Value : index.Value;
                return this[i];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                int i = index.IsFromEnd ? count - index.Value : index.Value;
                this[i] = value;
            }
        }

        // ---------- capacity / layout ----------
        private void EnsureCapacity(int additionalItems = 1)
        {
            if (count + additionalItems <= buffer.Length)
            {
                return;
            }

            int desiredCapacity = Math.Max(buffer.Length * 2, count + additionalItems);
            int offset = (desiredCapacity - count) / 2;
            
            ReplaceBuffer(buffer.Length, offset);
        }

        public void Justify()
        {
            if (count == 0 || head == 0 && (tail == count || tail == 0))
            {
                return;
            }

            int offset = (buffer.Length - count) / 2;
            ReplaceBuffer(buffer.Length, offset);
        }

        private void ReplaceBuffer(int newCapacity, int offset)
        {
            T[] next = ArrayPool<T>.Shared.Rent(newCapacity);

            int rightCount = Math.Min(buffer.Length - head, count);
            Array.Copy(buffer, head, next, offset, rightCount);
            Array.Copy(buffer, 0, next, offset + rightCount, count - rightCount);

            // swap
            T[] old = buffer;
            buffer = next;
            head = offset;
            tail = offset + count;

            ArrayPool<T>.Shared.Return(old, clearArray: clear_return_buffer);
        }
        
        /// <summary>
        /// Returns a span of the buffer; becomes invalid if buffer is mutated
        /// </summary>
        /// <returns></returns>
        public Span<T> AsSpan()
        {
            Justify();
            return new Span<T>(buffer, head, count);
        }

        // ---------- enumeration ----------
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: clear_return_buffer);
        }
    }
}
