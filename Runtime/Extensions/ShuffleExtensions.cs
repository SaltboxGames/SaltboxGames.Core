/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SaltboxGames.Core.Utilities;

namespace SaltboxGames.Core.Extensions
{
    public static class ShuffleExtensions
    {
        /// <summary>
        /// Simple in place fisher-yates shuffle
        /// </summary>
        public static void Shuffle<T>(this Span<T> span)
        {
            int n = span.Length;
            while (n > 1)
            {
                int k = DeterministicRandom.NextInt(n--);
                Swap(ref span[n], ref span[k]);
            }
        }
        
        /// <summary>
        /// Simple in place fisher-yates shuffle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<T>(this List<T> list)
        {
            Shuffle(list.AsSpan());
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }
    }
}
