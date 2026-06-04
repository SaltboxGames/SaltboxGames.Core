/*
 * Copyright (c) 2024 SaltboxGames
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Runtime.CompilerServices;
using SaltboxGames.Core.Utilities;

namespace SaltboxGames.Core.Extensions
{
    /// <summary>
    /// Provides allocation-free helpers for enum values.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the declaration index of the enum value, or -1 if the value is not declared.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to resolve.</param>
        /// <returns>The zero-based declaration index, or -1 when <paramref name="value"/> is not defined.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex<T>(this T value)
            where T : System.Enum
        {
            return EnumHelper<T>.GetIndex(value);
        }

        /// <summary>
        /// Converts the enum value to its 32-bit integer representation.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>The value converted to <see cref="int"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt<T>(this T value)
            where T : System.Enum
        {
            return EnumHelper<T>.ToInt32(value);
        }
    }
}
