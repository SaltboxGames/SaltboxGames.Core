// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
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
    /// <summary>
    /// Provides a pooled, double-ended circular buffer with efficient insertion and removal at both ends.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the buffer.</typeparam>
    public sealed partial class CircularBuffer<T> : IEnumerable<T>, IDisposable
    {
        private static readonly bool clear_return_buffer =
            RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        
        // Internal for zlinq enumerable things
        internal T[] buffer;
        internal int head;

        private int tail;
        private int count;

        /// <summary>
        /// Gets the number of active items in the buffer.
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Gets the current backing storage capacity.
        /// </summary>
        public int Capacity => buffer.Length;

        /// <summary>
        /// Gets a value indicating whether the buffer contains no active items.
        /// </summary>
        public bool IsEmpty => count == 0;

        /// <summary>
        /// Creates a new circular buffer with at least the requested capacity.
        /// </summary>
        /// <param name="capacity">The minimum initial capacity to rent from the shared array pool.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than one.</exception>
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
        /// <summary>
        /// Adds an item to the tail of the buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Push(T item)
        {
            EnsureCapacity();
            PushUnchecked(item);
        }

        /// <summary>
        /// Adds an item to the head of the buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Shift(T item)
        {
            EnsureCapacity();
            ShiftUnchecked(item);
        }

        /// <summary>
        /// Removes and returns the item at the tail of the buffer.
        /// </summary>
        /// <returns>The removed tail item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        public T Pop()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return PopUnchecked();
        }

        /// <summary>
        /// Removes and returns the item at the head of the buffer.
        /// </summary>
        /// <returns>The removed head item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        public T Unshift()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return UnshiftUnchecked();
        }

        // ---------- range operations ----------
        /// <summary>
        /// Adds a range of items to the tail of the buffer.
        /// </summary>
        /// <param name="items">The items to add.</param>
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

        /// <summary>
        /// Adds a range of items to the head of the buffer while preserving their order.
        /// </summary>
        /// <param name="items">The items to add.</param>
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

        /// <summary>
        /// Removes items from the tail of the buffer into a destination span.
        /// </summary>
        /// <param name="destination">The span that receives removed items.</param>
        /// <returns>The number of items removed.</returns>
        public int PopRange(Span<T> destination)
        {
            int toRemove = Math.Min(destination.Length, count);
            for (int i = toRemove - 1; i >= 0; i--)
            {
                destination[i] = PopUnchecked();
            }
            return toRemove;
        }

        /// <summary>
        /// Removes items from the head of the buffer into a destination span.
        /// </summary>
        /// <param name="destination">The span that receives removed items.</param>
        /// <returns>The number of items removed.</returns>
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
        /// <summary>
        /// Returns the item at the head of the buffer without removing it.
        /// </summary>
        /// <returns>The current head item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        public T PeekHead()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return this[0];
        }

        /// <summary>
        /// Returns the item at the tail of the buffer without removing it.
        /// </summary>
        /// <returns>The current tail item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        public T PeekTail()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Buffer is empty");
            }
            return this[^1];
        }

        // ---------- indexers ----------
        /// <summary>
        /// Gets or sets an item by its zero-based logical index from the head of the buffer.
        /// </summary>
        /// <param name="index">The zero-based logical index.</param>
        /// <returns>The item at the requested logical index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is outside the active range.</exception>
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

        /// <summary>
        /// Gets or sets an item by logical index, including indices from the end.
        /// </summary>
        /// <param name="index">The logical index to access.</param>
        /// <returns>The item at the requested logical index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is outside the active range.</exception>
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
            
            ReplaceBuffer(desiredCapacity, offset);
        }

        /// <summary>
        /// Moves active items so they occupy one contiguous range in the backing array.
        /// </summary>
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
        /// Returns a span over the active buffer items after making them contiguous.
        /// </summary>
        /// <returns>A span over the active items.</returns>
        /// <remarks>The returned span becomes invalid after the buffer is mutated.</remarks>
        public Span<T> AsSpan()
        {
            Justify();
            return new Span<T>(buffer, head, count);
        }

        // ---------- enumeration ----------
        /// <summary>
        /// Returns an enumerator over the active items in logical order from head to tail.
        /// </summary>
        /// <returns>An enumerator over the active items.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns the backing array to the shared array pool.
        /// </summary>
        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(buffer, clearArray: clear_return_buffer);
        }
    }
}
