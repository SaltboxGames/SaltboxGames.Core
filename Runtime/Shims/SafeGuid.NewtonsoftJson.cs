// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if NEWTONSOFT_JSON

using System;
using Newtonsoft.Json;

namespace SaltboxGames.Core.Shims
{
    /// <summary>
    /// Adds Newtonsoft.Json string serialization support to <see cref="SafeGuid"/>.
    /// </summary>
    [JsonConverter(typeof(SafeGuidNewtonsoftJsonConverter))]
    public partial struct SafeGuid { }

    internal sealed class SafeGuidNewtonsoftJsonConverter : JsonConverter<SafeGuid>
    {
        public override void WriteJson(JsonWriter writer, SafeGuid value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override SafeGuid ReadJson(
            JsonReader reader,
            Type objectType,
            SafeGuid existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.Value is Guid guid)
            {
                return guid;
            }

            if (reader.Value is string text && SafeGuid.TryParse(text, out SafeGuid parsed))
            {
                return parsed;
            }

            throw new JsonSerializationException($"Expected a GUID string while deserializing {nameof(SafeGuid)} at path '{reader.Path}'.");
        }
    }
}

#endif
