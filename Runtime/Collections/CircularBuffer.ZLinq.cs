// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if ZLINQ

using System;
using ZLinq;

namespace SaltboxGames.Core.Collections
{
    /// <summary>
    /// Adds ZLinq value enumerable support to <see cref="CircularBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the buffer.</typeparam>
    public sealed partial class CircularBuffer<T> : IValueEnumerable<CircularBufferEnumerator<T>, T>
    {
        /// <summary>
        /// Creates a ZLinq value enumerable over the active items in the buffer.
        /// </summary>
        /// <returns>A value enumerable for the buffer.</returns>
        public ValueEnumerable<CircularBufferEnumerator<T>, T> AsValueEnumerable()
        {
            return new ValueEnumerable<CircularBufferEnumerator<T>, T>(new CircularBufferEnumerator<T>(this));
        }
    }

    /// <summary>
    /// Provides a ZLinq value enumerator for <see cref="CircularBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the buffer.</typeparam>
    public struct CircularBufferEnumerator<T> : IValueEnumerator<T>
    {
        private readonly CircularBuffer<T> target;
        private readonly int count;
        
        private int index;
        
        /// <summary>
        /// Creates an enumerator for the specified circular buffer.
        /// </summary>
        /// <param name="target">The buffer to enumerate.</param>
        public CircularBufferEnumerator(CircularBuffer<T> target)
        {
            this.target = target;
            index = 0;
            count = target.Count;
        }

        /// <summary>
        /// Gets the next item in logical buffer order.
        /// </summary>
        /// <param name="current">The next item when one is available; otherwise the default value.</param>
        /// <returns><see langword="true"/> when an item was produced; otherwise <see langword="false"/>.</returns>
        public bool TryGetNext(out T current)
        {
            if (index >= count)
            {
                current = default!;
                return false;
            }

            current = target[index];
            index++;
            return true;
        }

        /// <summary>
        /// Gets the number of items available from this enumerator without enumerating.
        /// </summary>
        /// <param name="count">The number of items available.</param>
        /// <returns>Always <see langword="true"/>.</returns>
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = this.count;
            return true;
        }

        /// <summary>
        /// Attempts to expose the active buffer contents as a contiguous span.
        /// </summary>
        /// <param name="span">The span over the active buffer contents.</param>
        /// <returns>Always <see langword="true"/>.</returns>
        /// <remarks>The returned span becomes invalid after the buffer is mutated.</remarks>
        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = target.AsSpan();
            return true;
        }

        /// <summary>
        /// Copies the active buffer contents to a destination span at the specified offset.
        /// </summary>
        /// <param name="destination">The span that receives the copied items.</param>
        /// <param name="offset">The destination offset where copying begins.</param>
        /// <returns><see langword="true"/> when the destination has enough space; otherwise <see langword="false"/>.</returns>
        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            if (destination.Length - offset.Value < count)
            {
                return false;
            }

            int rightCount = Math.Min(target.buffer.Length - target.head, count);
            
            target.buffer
                .AsSpan(target.head, rightCount)
                .CopyTo(destination[offset.Value..]);

            target.buffer
                .AsSpan(0, count - rightCount)
                .CopyTo(destination[(offset.Value + rightCount)..]);

            return true;
        }

        /// <summary>
        /// Releases enumerator resources.
        /// </summary>
        public void Dispose() { }
    }
}

#endif
