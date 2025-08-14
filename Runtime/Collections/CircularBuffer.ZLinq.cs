/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if zlinq

using System;
using ZLinq;

namespace SaltboxGames.Core.Collections
{
    public sealed partial class CircularBuffer<T> : IValueEnumerable<CircularBufferEnumerator<T>, T>
    {
        public ValueEnumerable<CircularBufferEnumerator<T>, T> AsValueEnumerable()
        {
            return new ValueEnumerable<CircularBufferEnumerator<T>, T>(new CircularBufferEnumerator<T>(this));
        }
    }

    public struct CircularBufferEnumerator<T> : IValueEnumerator<T>
    {
        private readonly CircularBuffer<T> target;
        private readonly int count;
        
        private int index;
        
        public CircularBufferEnumerator(CircularBuffer<T> target)
        {
            this.target = target;
            index = 0;
            count = target.Count;
        }

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

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = this.count;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = target.AsSpan();
            return true;
        }

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
        public void Dispose() { }
    }
}

#endif
