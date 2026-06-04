/*
 * Copyright (c) 2024 SaltboxGames
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;

namespace SaltboxGames.Core.Utilities
{
    /// <summary>
    /// Provides a shared deterministic XorShift-style random number generator.
    /// </summary>
    public static class DeterministicRandom
    {
        private static int _state = 1;
        
        static DeterministicRandom()
        {
            // this is dumb, but fast and fun
            Span<byte> buffer = stackalloc byte[4];
            RandomNumberGenerator.Fill(buffer);

            int value = Unsafe.As<byte, int>(ref buffer[0]);
            Seed(value | 1);
        }
        
        /// <summary>
        /// Sets the generator state.
        /// </summary>
        /// <param name="seed">The seed to use. The value is forced odd to avoid the zero state.</param>
        public static void Seed(int seed)
        {
            seed |= 1;
            Interlocked.Exchange(ref _state, seed);
        }

        /// <summary>
        /// Gets the next unsigned 32-bit value.
        /// </summary>
        /// <returns>The next generated value.</returns>
        public static uint NextUInt()
        {
            while (true)
            {
                int original = _state;
                int next = original;
                
                unchecked
                {
                    next ^= next << 13;
                    next ^= (int)((uint)next >> 17);
                    next ^= next << 5;
                }

                if (Interlocked.CompareExchange(ref _state, next, original) == original)
                {
                    return unchecked((uint)next);
                }
            }
        }

        /// <summary>
        /// Gets the next integer in the range <c>[0, max)</c>.
        /// </summary>
        /// <param name="max">The exclusive upper bound.</param>
        /// <returns>A generated integer less than <paramref name="max"/>.</returns>
        public static int NextInt(int max) => (int)(NextUInt() % max);

        /// <summary>
        /// Gets the next integer in the range <c>[min, max)</c>.
        /// </summary>
        /// <param name="min">The inclusive lower bound.</param>
        /// <param name="max">The exclusive upper bound.</param>
        /// <returns>A generated integer in the requested range.</returns>
        public static int NextInt(int min, int max) => min + NextInt(max - min);

        /// <summary>
        /// Gets the next single-precision floating point value.
        /// </summary>
        /// <returns>A generated floating point value.</returns>
        public static float NextFloat()
        {
            uint rnd = NextUInt() & 0x7FFFFFFF;
            return Unsafe.As<uint, float>(ref rnd) / int.MaxValue;
        }

        /// <summary>
        /// Gets the next single-precision floating point value between two bounds.
        /// </summary>
        /// <param name="min">The lower bound.</param>
        /// <param name="max">The upper bound.</param>
        /// <returns>A generated floating point value in the requested range.</returns>
        public static float NextFloat(float min, float max)
        {
            return min + NextFloat() * (max - min);
        }

        /// <summary>
        /// Gets the next double-precision floating point value.
        /// </summary>
        /// <returns>A generated double-precision floating point value.</returns>
        public static double NextDouble()
        {
            ulong hi = NextUInt();
            ulong lo = NextUInt();
            ulong combined = (hi << 32) | lo;
            return combined / (double)ulong.MaxValue;
        }

        /// <summary>
        /// Gets the next double-precision floating point value between two bounds.
        /// </summary>
        /// <param name="min">The lower bound.</param>
        /// <param name="max">The upper bound.</param>
        /// <returns>A generated double-precision floating point value in the requested range.</returns>
        public static double NextDouble(double min, double max)
        {
            return min + NextDouble() * (max - min);
        }

        /// <summary>
        /// Gets the next unsigned 64-bit value.
        /// </summary>
        /// <returns>The next generated 64-bit value.</returns>
        public static ulong NextULong()
        {
            ulong hi = NextUInt();
            ulong lo = NextUInt();
            return (hi << 32) | lo;
        }
    }
}
