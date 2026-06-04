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
using SaltboxGames.Core.Extensions;

namespace SaltboxGames.Core.Utilities
{
    /// <summary>
    /// Provides helper methods for string collections.
    /// </summary>
    public static class StringUtilities
    {
#if !ENABLE_IL2CPP && (UNITY_6000_0_OR_NEWER || UNITY_EDITOR)
        /// <summary>
        /// Finds the longest prefix shared by all strings in a list.
        /// </summary>
        /// <param name="strings">The strings to inspect.</param>
        /// <returns>The longest common prefix, or an empty string when no prefix is shared.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string LongestCommonPrefix(List<string> strings)
        {
            Span<string> span = strings.AsSpan();
            return LongestCommonPrefix(span);
        }
#else
        /// <summary>
        /// Finds the longest prefix shared by all strings in a list.
        /// </summary>
        /// <param name="strings">The strings to inspect.</param>
        /// <returns>The longest common prefix, or an empty string when no prefix is shared.</returns>
        public static string LongestCommonPrefix(List<string> strings)
        {
            if (strings.Count == 0)
            {
                return "";
            }

            string prefix = strings[0];
            for (int index = 1; index < strings.Count; index++)
            {
                string str = strings[index];

                int i = 0;
                while (i < prefix.Length && i < str.Length && prefix[i] == str[i])
                {
                    i++;
                }

                prefix = prefix.Substring(0, i);

                if (prefix.Length == 0)
                    break;
            }
            return prefix;
        }        
#endif
        /// <summary>
        /// Finds the longest prefix shared by all strings in a span.
        /// </summary>
        /// <param name="strings">The strings to inspect.</param>
        /// <returns>The longest common prefix, or an empty string when no prefix is shared.</returns>
        public static string LongestCommonPrefix(ReadOnlySpan<string> strings)
        {
            if (strings.Length == 0)
            {
                return "";
            }
            
            string prefix = strings[0];
            for (int index = 1; index < strings.Length; index++)
            {
                string str = strings[index];
                
                int i = 0;
                while (i < prefix.Length && i < str.Length && prefix[i] == str[i])
                {
                    i++;
                }
                prefix = prefix.Substring(0, i);
                if (prefix == "")
                {
                    break;
                }
            }
            return prefix;
        }
    }
}
