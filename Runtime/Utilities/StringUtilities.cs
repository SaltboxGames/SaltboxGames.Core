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
using SaltboxGames.Core.Extensions;

namespace SaltboxGames.Core.Utilities
{
    public static class StringUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string LongestCommonPrefix(List<string> strings)
        {
            Span<string> span = strings.AsSpan();
            return LongestCommonPrefix(span);
        }
        
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
