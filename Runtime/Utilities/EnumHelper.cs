/*
 * Copyright (c) 2024 SaltboxGames
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SaltboxGames.Core.Utilities
{
    /// <summary>
    /// Caches values and lookup delegates for enum type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The enum type to cache metadata for.</typeparam>
    public static class EnumHelper<T>
        where T : Enum
    {
        private const int LinearSearchThreshold = 8;

        /// <summary>
        /// Gets the declared values for enum type <typeparamref name="T"/>.
        /// </summary>
        public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));

        /// <summary>
        /// Gets the number of declared values for enum type <typeparamref name="T"/>.
        /// </summary>
        public static readonly int Count = Values.Length;

        /// <summary>
        /// Converts enum values to their 32-bit integer representation.
        /// </summary>
        public static readonly Func<T, int> ToInt32 = CreateToInt32();

        /// <summary>
        /// Resolves enum values to their zero-based declaration index, or -1 when undefined.
        /// </summary>
        public static readonly Func<T, int> GetIndex = CreateGetIndex();

        private static Func<T, int> CreateToInt32()
        {
            Type backing = Enum.GetUnderlyingType(typeof(T));
            if (backing == typeof(int))
            {
                return static value => Unsafe.As<T, int>(ref value);
            }

            if (backing == typeof(byte))
            {
                return static value => Unsafe.As<T, byte>(ref value);
            }

            if (backing == typeof(sbyte))
            {
                return static value => Unsafe.As<T, sbyte>(ref value);
            }

            if (backing == typeof(short))
            {
                return static value => Unsafe.As<T, short>(ref value);
            }

            if (backing == typeof(ushort))
            {
                return static value => Unsafe.As<T, ushort>(ref value);
            }

            if (backing == typeof(uint))
            {
                return static value => checked((int)Unsafe.As<T, uint>(ref value));
            }

            if (backing == typeof(long))
            {
                return static value => checked((int)Unsafe.As<T, long>(ref value));
            }

            if (backing == typeof(ulong))
            {
                return static value => checked((int)Unsafe.As<T, ulong>(ref value));
            }

            throw new InvalidOperationException($"Unsupported enum backing type: {backing}");
        }
        
        // [Jon] Mildly criminal but IL2CPP safe, and reasonably fast.
        // If this ever matters, we can replace everything in here with a source generator.
        private static Func<T, int> CreateGetIndex()
        {
            int count = Values.Length;
            if (count == 0)
            {
                return static _ => -1;
            }

            int min = int.MaxValue;
            int max = int.MinValue;

            bool aligned = true;
            int[] rawValues = new int[count];

            for (int i = 0; i < count; i++)
            {
                int value = ToInt32(Values[i]);
                rawValues[i] = value;

                if (value != i)
                {
                    aligned = false;
                }

                if (value < min)
                {
                    min = value;
                }

                if (value > max)
                {
                    max = value;
                }
            }

            // Perfect case:
            // enum Foo { A = 0, B = 1, C = 2 }
            if (aligned)
            {
                return ToInt32;
            }
            
            // Sequential offset: -2, -1, 0 or 10, 11, 12
            bool sequential = true;
            for (int i = 0; i < count; i++)
            {
                if (rawValues[i] != min + i)
                {
                    sequential = false;
                    break;
                }
            }

            if (sequential)
            {
                return value => ToInt32(value) - min;
            }

            long range = (long)max - min + 1;

            // Dense enum:
            // enum Foo { A = 1, B = 5, C = 3 }
            if (range <= count * 2L)
            {
                int[] lookup = new int[(int)range];
                Array.Fill(lookup, -1);

                for (int i = 0; i < count; i++)
                {
                    lookup[rawValues[i] - min] = i;
                }

                return value =>
                {
                    int raw = ToInt32(value);
                    int offset = raw - min;

                    if ((uint)offset < (uint)lookup.Length)
                    {
                        return lookup[offset];
                    }
                    return -1;
                };
            }

            // Sparse enum: (small)
            // enum Foo { A = 1, B = 100, C = 10000 }
            if (count <= LinearSearchThreshold)
            {
                return value =>
                {
                    int raw = ToInt32(value);
                    for (int i = rawValues.Length - 1; i >= 0; i--)
                    {
                        if (rawValues[i] == raw)
                        {
                            return i;
                        }
                    }

                    return -1;
                };
            }

            // Sparse enum: (large)
            // enum Foo { A = 1, B = 100, C = 10000, .... Z = 45}
            var lookupDictionary = new Dictionary<int, int>(count);
            for (int i = 0; i < count; i++)
            {
                lookupDictionary[rawValues[i]] = i;
            }

            return value =>
            {
                int raw = ToInt32(value);
                return lookupDictionary.GetValueOrDefault(raw, -1);
            };
        }
    }
}
