# Shims

Shim types live under `SaltboxGames.Core.Shims`.

## SafeGuid.cs

`SafeGuid` is a serializable `Guid` wrapper intended to work across Unity runtime, Unity editor, .NET, and optional MemoryPack serialization.

Key behavior:

- Stores the value in a 16-byte explicit-layout struct.
- Provides implicit conversions to and from `System.Guid`.
- Provides Unity editor conversions to and from `UnityEditor.GUID` when `UNITY_EDITOR` is defined.
- Uses Unity `[SerializeField]` fields when `UNITY_2019_4_OR_NEWER` is defined.
- Adds MemoryPack attributes when `MEMORY_PACK` is defined.
- Formats `ToString()` with the `N` format to match Unity editor GUID strings.

Key members:

- `NewGuid()`: creates a new value.
- `Parse(ReadOnlySpan<char> input)`: parses a GUID string.
- `TryParse(ReadOnlySpan<char> input, out SafeGuid result)`: attempts to parse a GUID string.
- `Decompose(out int s1, out int s2, out int s3, out int s4)`: exposes the serialized integer segments.
- `FromSegments(int s1, int s2, int s3, int s4)`: reconstructs a value from serialized segments.
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
