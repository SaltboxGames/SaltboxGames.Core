/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
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
    /// Simple Xor32 random~ish number generator.
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
        
        public static void Seed(int seed)
        {
            seed |= 1;
            Interlocked.Exchange(ref _state, seed);
        }

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

        public static int NextInt(int max) => (int)(NextUInt() % max);
        public static int NextInt(int min, int max) => min + NextInt(max - min);

        public static float NextFloat()
        {
            uint rnd = NextUInt() & 0x7FFFFFFF;
            return Unsafe.As<uint, float>(ref rnd) / int.MaxValue;
        }

        public static float NextFloat(float min, float max)
        {
            return min + NextFloat() * (max - min);
        }

        public static double NextDouble()
        {
            ulong hi = NextUInt();
            ulong lo = NextUInt();
            ulong combined = (hi << 32) | lo;
            return combined / (double)ulong.MaxValue;
        }

        public static double NextDouble(double min, double max)
        {
            return min + NextDouble() * (max - min);
        }

        public static ulong NextULong()
        {
            ulong hi = NextUInt();
            ulong lo = NextUInt();
            return (hi << 32) | lo;
        }
    }
}