# Utilities

Utility helpers live under `SaltboxGames.Core.Utilities`.

## DeterministicRandom.cs

`DeterministicRandom` is a small static random source based on XorShift-style state transitions.

Key members:

- `Seed(int seed)`: sets the shared state. The seed is forced odd to avoid the zero state.
- `NextUInt()`: returns the next unsigned 32-bit value.
- `NextInt(int max)`: returns a value in `[0, max)`.
- `NextInt(int min, int max)`: returns a value in `[min, max)`.
- `NextFloat()`: returns a floating point value derived from the generator.
- `NextFloat(float min, float max)`: returns a floating point value between `min` and `max`.
- `NextDouble()`: returns a double derived from two generated `uint` values.
- `NextDouble(double min, double max)`: returns a double between `min` and `max`.
- `NextULong()`: returns a 64-bit value derived from two generated `uint` values.

The generator uses shared static state and atomic updates. It is deterministic after `Seed`, but it is not a cryptographic random source.

## Reflection.Fields.cs

Provides cached compiled delegates for instance field access.

Key members:

- `GetFieldGetter<TTarget, TValue>(TTarget target, string fieldName)`: returns a zero-argument getter bound to `target`.
- `GetFieldSetter<TTarget, TValue>(TTarget target, string fieldName)`: returns a one-argument setter bound to `target`.
- `GetFieldGetter<TTarget, TValue>(string fieldName)`: returns a reusable getter delegate that accepts the target instance.
- `GetFieldSetter<TTarget, TValue>(string fieldName)`: returns a reusable setter delegate that accepts the target instance and value.

See [Reflection Utilities](./reflection.md) for examples and target restrictions.

## Reflection.Properties.cs

Provides cached compiled delegates for instance property getters.

Key members:

- `GetPropertyGetter<TTarget, TValue>(TTarget target, string propertyName)`: returns a zero-argument getter bound to `target`.
- `GetPropertyGetter<TTarget, TValue>(string propertyName)`: returns a reusable getter delegate that accepts the target instance.

See [Reflection Utilities](./reflection.md) for examples and target restrictions.

## SpanUtilities.cs

`SpanUtilities` contains small helpers for span and reference manipulation.

Key members:

- `Swap<T>(ref T a, ref T b)`: swaps two references.
- `Swap<T>(Span<T> array, int a, int b)`: swaps two elements in a span by index.

Example:

```csharp
Span<int> values = stackalloc[] { 1, 2 };
SpanUtilities.Swap(values, 0, 1);
```

## StringUtilities.cs

`StringUtilities` contains string algorithms.

Key members:

- `LongestCommonPrefix(List<string> strings)`: finds the common prefix for a list.
- `LongestCommonPrefix(ReadOnlySpan<string> strings)`: finds the common prefix for a span.

The list overload uses span access where available and a direct list loop elsewhere.

Example:

```csharp
var prefix = StringUtilities.LongestCommonPrefix(
    new List<string> { "package", "packet", "pack" });
// prefix == "pack"
```
