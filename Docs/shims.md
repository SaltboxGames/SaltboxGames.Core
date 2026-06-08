# Shims

Shim types live under `SaltboxGames.Core.Shims`.

## SafeGuid.cs

`SafeGuid` is a serializable `Guid` wrapper intended to work across Unity runtime, Unity editor, .NET, and optional MemoryPack/Newtonsoft.Json serialization.

Key behavior:

- Stores the value in a 16-byte explicit-layout struct.
- Provides implicit conversions to and from `System.Guid`.
- Formats `ToString()` with the `N` format to match Unity editor GUID strings.

Key members:

- `NewGuid()`: creates a new value.
- `Parse(ReadOnlySpan<char> input)`: parses a GUID string.
- `TryParse(ReadOnlySpan<char> input, out SafeGuid result)`: attempts to parse a GUID string.
- Equality operators and `IEquatable<SafeGuid>` support.
- `IFormattable.ToString(string format, IFormatProvider provider)` support.

Example:

```csharp
using SaltboxGames.Core.Shims;

SafeGuid id = SafeGuid.NewGuid();
Guid systemGuid = id;
SafeGuid roundTrip = systemGuid;
string text = id.ToString();
```

## SafeGuid.Unity.cs

When `UNITY_2019_4_OR_NEWER` is defined, `SafeGuid` includes Unity serialization support so values can be stored on serialized Unity objects.

## SafeGuid.UnityEditor.cs

When `UNITY_EDITOR` is defined, `SafeGuid` provides explicit conversions to and from `UnityEditor.GUID`.

## SafeGuid.MemoryPack.cs

When `MEMORY_PACK` is defined, `SafeGuid` is annotated for MemoryPack serialization support.

## SafeGuid.NewtonsoftJson.cs

When the `NEWTONSOFT_JSON` compilation symbol is defined, `SafeGuid` is annotated with a Newtonsoft.Json converter.

The converter serializes `SafeGuid` as a JSON string using the same `N` format returned by `SafeGuid.ToString()`: 32 lowercase hexadecimal digits without hyphens.

Example JSON:

```json
"f6f0a5f7a64d48a7a3d6f4e7eacb5a21"
```

During deserialization, the converter accepts either an existing `System.Guid` value from the reader or a string accepted by `SafeGuid.TryParse`. Invalid values throw a `JsonSerializationException`.
